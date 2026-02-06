#!/usr/bin/env python3
"""Check Element assets and ElementCombo assets for tier-consistent combos.

Scans Unity .asset files, identifies Element and ElementCombo assets by matching
m_Script GUIDs, and validates:
- elements are in folders matching their tier (Primitive/Tier1/Tier2)
- combos use valid element references
- combos within the same tier produce the next tier
- missing combos within the same tier (optional)
- database asset includes all elements and combos (optional)
"""

from __future__ import annotations

import argparse
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Tuple

GUID_RE = re.compile(r"guid:\s*([0-9a-fA-F]+)")
ELEMENT_NAME_RE = re.compile(r"^\s*elementName:\s*(.*)$")
ELEMENT_TIER_RE = re.compile(r"^\s*elementTier:\s*(\d+)\s*$")
BEHAVIORS_INLINE_RE = re.compile(r"^\s*behaviors:\s*\[\]\s*$")
BEHAVIORS_START_RE = re.compile(r"^\s*behaviors:\s*$")
BEHAVIOR_ITEM_RE = re.compile(r"^\s*-\s+")
DATABASE_ELEMENTS_START_RE = re.compile(r"^\s*elements:\s*$")
DATABASE_COMBOS_START_RE = re.compile(r"^\s*elementCombos:\s*$")
DATABASE_LIST_ITEM_RE = re.compile(r"^\s*-\s+")
BEHAVIOR_TAGTYPE_NULL_RE = re.compile(r"TagType\s*\{[^}]*\}\s*=\s*null|=>\s*null")
BEHAVIOR_REMOVE_CALL_RE = re.compile(r"\bRemoveBehavior\s*\(")

TIER_LABELS = {0: "Primitive", 1: "Tier1", 2: "Tier2"}
TIER_FOLDER_ALIASES = {
    "primitive": 0,
    "tier1": 1,
    "tier2": 2,
}


@dataclass(frozen=True)
class ElementInfo:
    guid: str
    name: str
    tier: Optional[int]
    path: Path
    behavior_count: int


@dataclass(frozen=True)
class ComboInfo:
    guid: str
    input_a: Optional[str]
    input_b: Optional[str]
    result: Optional[str]
    path: Path


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="ignore")


def find_script_guid(assets_root: Path, script_name: str) -> Optional[str]:
    for meta_path in assets_root.rglob(f"{script_name}.meta"):
        for line in read_text(meta_path).splitlines():
            if line.startswith("guid:"):
                return line.split("guid:", 1)[1].strip()
    return None


def extract_guid_from_line(line: str) -> Optional[str]:
    match = GUID_RE.search(line)
    return match.group(1) if match else None


def extract_folder_tier(path: Path) -> Optional[int]:
    for part in path.parts:
        normalized = part.replace(" ", "").lower()
        if normalized in TIER_FOLDER_ALIASES:
            return TIER_FOLDER_ALIASES[normalized]
    return None


def parse_element_asset(path: Path) -> Tuple[Optional[str], Optional[int], int]:
    name: Optional[str] = None
    tier: Optional[int] = None
    behavior_count = 0
    in_behaviors = False
    for line in read_text(path).splitlines():
        if name is None:
            name_match = ELEMENT_NAME_RE.match(line)
            if name_match:
                name = name_match.group(1).strip().strip('"')
        if tier is None:
            tier_match = ELEMENT_TIER_RE.match(line)
            if tier_match:
                tier = int(tier_match.group(1))
        if BEHAVIORS_INLINE_RE.match(line):
            in_behaviors = False
        elif BEHAVIORS_START_RE.match(line):
            in_behaviors = True
        elif in_behaviors and BEHAVIOR_ITEM_RE.match(line):
            behavior_count += 1
        elif in_behaviors and line and not line.startswith(" "):
            in_behaviors = False

    return name, tier, behavior_count


def parse_combo_asset(path: Path) -> Tuple[Optional[str], Optional[str], Optional[str]]:
    input_a = input_b = result = None
    for line in read_text(path).splitlines():
        if "elementA:" in line:
            input_a = extract_guid_from_line(line)
        elif "elementB:" in line:
            input_b = extract_guid_from_line(line)
        elif "resultElement:" in line:
            result = extract_guid_from_line(line)
        if input_a and input_b and result:
            break
    return input_a, input_b, result


def parse_behavior_guids(path: Path) -> List[str]:
    behavior_guids: List[str] = []
    in_behaviors = False
    for line in read_text(path).splitlines():
        if BEHAVIORS_START_RE.match(line):
            in_behaviors = True
            continue
        if in_behaviors and BEHAVIOR_ITEM_RE.match(line):
            guid = extract_guid_from_line(line)
            if guid:
                behavior_guids.append(guid)
        elif in_behaviors and line and not line.startswith(" "):
            in_behaviors = False
    return behavior_guids


def build_element_index(
    assets_root: Path, element_guid: str
) -> Dict[str, ElementInfo]:
    elements: Dict[str, ElementInfo] = {}
    for asset_path in assets_root.rglob("*.asset"):
        if element_guid not in read_text(asset_path):
            continue
        # Confirm script GUID match
        if element_guid not in read_text(asset_path):
            continue

        name, tier, behavior_count = parse_element_asset(asset_path)
        name = name or asset_path.stem
        elements[asset_path.as_posix()] = ElementInfo(
            guid=asset_path.as_posix(),
            name=name,
            tier=tier,
            path=asset_path,
            behavior_count=behavior_count,
        )

    # Rebuild mapping by real GUID from meta file if present
    guid_map: Dict[str, ElementInfo] = {}
    for element in elements.values():
        meta_path = element.path.with_suffix(element.path.suffix + ".meta")
        if meta_path.exists():
            for line in read_text(meta_path).splitlines():
                if line.startswith("guid:"):
                    guid = line.split("guid:", 1)[1].strip()
                    guid_map[guid] = ElementInfo(
                        guid=guid,
                        name=element.name,
                        tier=element.tier,
                        path=element.path,
                        behavior_count=element.behavior_count,
                    )
                    break
    return guid_map


def build_combo_index(
    assets_root: Path, combo_guid: str
) -> List[ComboInfo]:
    combos: List[ComboInfo] = []
    for asset_path in assets_root.rglob("*.asset"):
        if combo_guid not in read_text(asset_path):
            continue
        input_a, input_b, result = parse_combo_asset(asset_path)
        meta_path = asset_path.with_suffix(asset_path.suffix + ".meta")
        combo_asset_guid: Optional[str] = None
        if meta_path.exists():
            for line in read_text(meta_path).splitlines():
                if line.startswith("guid:"):
                    combo_asset_guid = line.split("guid:", 1)[1].strip()
                    break
        combos.append(
            ComboInfo(
                guid=combo_asset_guid or asset_path.as_posix(),
                input_a=input_a,
                input_b=input_b,
                result=result,
                path=asset_path,
            )
        )
    return combos


def build_guid_to_asset_path(assets_root: Path) -> Dict[str, Path]:
    guid_map: Dict[str, Path] = {}
    for meta_path in assets_root.rglob("*.meta"):
        for line in read_text(meta_path).splitlines():
            if line.startswith("guid:"):
                guid = line.split("guid:", 1)[1].strip()
                guid_map[guid] = meta_path.with_suffix("")
                break
    return guid_map


def parse_database_asset(path: Path) -> Tuple[set[str], set[str]]:
    element_guids: set[str] = set()
    combo_guids: set[str] = set()
    in_elements = False
    in_combos = False

    for line in read_text(path).splitlines():
        if DATABASE_ELEMENTS_START_RE.match(line):
            in_elements = True
            in_combos = False
            continue
        if DATABASE_COMBOS_START_RE.match(line):
            in_combos = True
            in_elements = False
            continue

        if in_elements and DATABASE_LIST_ITEM_RE.match(line):
            guid = extract_guid_from_line(line)
            if guid:
                element_guids.add(guid)
        elif in_combos and DATABASE_LIST_ITEM_RE.match(line):
            guid = extract_guid_from_line(line)
            if guid:
                combo_guids.add(guid)

        if (in_elements or in_combos) and line and not line.startswith(" "):
            in_elements = False
            in_combos = False

    return element_guids, combo_guids


def unordered_pair(a: str, b: str) -> Tuple[str, str]:
    return (a, b) if a < b else (b, a)


def check_combos(
    elements: Dict[str, ElementInfo],
    combos: List[ComboInfo],
    require_all_pairs: bool,
    require_combo_per_element: bool,
    require_behaviors: bool,
    database_asset: Optional[Path],
    require_instant_removal: bool,
    assets_root: Path,
) -> int:
    errors = 0

    elements_by_tier: Dict[int, List[ElementInfo]] = {0: [], 1: [], 2: []}
    folder_mismatches: List[str] = []

    for element in elements.values():
        folder_tier = extract_folder_tier(element.path)
        final_tier = element.tier if element.tier is not None else folder_tier
        if final_tier is not None:
            elements_by_tier.setdefault(final_tier, []).append(
                ElementInfo(
                    guid=element.guid,
                    name=element.name,
                    tier=final_tier,
                    path=element.path,
                    behavior_count=element.behavior_count,
                )
            )
        if element.tier is not None and folder_tier is not None:
            if element.tier != folder_tier:
                folder_mismatches.append(
                    f"{element.path} tier={TIER_LABELS.get(element.tier, element.tier)} "
                    f"folder={TIER_LABELS.get(folder_tier, folder_tier)}"
                )

    if folder_mismatches:
        print("Tier folder mismatches:")
        for entry in folder_mismatches:
            print(f"  - {entry}")
        errors += len(folder_mismatches)

    combo_lookup: Dict[Tuple[str, str], ComboInfo] = {}
    combo_usage: Dict[str, int] = {}
    for combo in combos:
        if not combo.input_a or not combo.input_b:
            print(f"Combo {combo.path} missing input element references.")
            errors += 1
            continue
        combo_lookup[unordered_pair(combo.input_a, combo.input_b)] = combo
        combo_usage[combo.input_a] = combo_usage.get(combo.input_a, 0) + 1
        combo_usage[combo.input_b] = combo_usage.get(combo.input_b, 0) + 1

    # Ensure combo assets live under the tier folder they target (if detectable).
    for combo in combos:
        if not (combo.input_a and combo.input_b):
            continue
        input_a = elements.get(combo.input_a)
        input_b = elements.get(combo.input_b)
        if not input_a or not input_b:
            continue
        if input_a.tier is None or input_b.tier is None:
            continue
        if input_a.tier != input_b.tier:
            continue
        combo_folder_tier = extract_folder_tier(combo.path)
        if combo_folder_tier is None:
            continue
        if combo_folder_tier != input_a.tier:
            print(
                f"Combo {combo.path} is in {TIER_LABELS.get(combo_folder_tier, combo_folder_tier)} "
                f"but inputs are {TIER_LABELS.get(input_a.tier, input_a.tier)}"
            )
            errors += 1

    # Validate combo tiering
    for combo in combos:
        if not (combo.input_a and combo.input_b and combo.result):
            continue
        input_a = elements.get(combo.input_a)
        input_b = elements.get(combo.input_b)
        result = elements.get(combo.result)
        if not input_a or not input_b or not result:
            print(f"Combo {combo.path} references missing elements.")
            errors += 1
            continue
        if input_a.tier is None or input_b.tier is None or result.tier is None:
            continue
        if input_a.tier != input_b.tier:
            print(
                f"Combo {combo.path} mixes tiers: "
                f"{TIER_LABELS.get(input_a.tier, input_a.tier)} + "
                f"{TIER_LABELS.get(input_b.tier, input_b.tier)}"
            )
            errors += 1
            continue
        expected = min(input_a.tier + 1, max(TIER_LABELS))
        if result.tier != expected:
            print(
                f"Combo {combo.path} result tier mismatch: "
                f"expected {TIER_LABELS.get(expected, expected)}, "
                f"got {TIER_LABELS.get(result.tier, result.tier)}"
            )
            errors += 1

    if require_all_pairs:
        for tier, tier_elements in elements_by_tier.items():
            if len(tier_elements) < 2:
                continue
            missing_pairs: List[str] = []
            for i in range(len(tier_elements)):
                for j in range(i, len(tier_elements)):
                    a = tier_elements[i]
                    b = tier_elements[j]
                    if unordered_pair(a.guid, b.guid) not in combo_lookup:
                        missing_pairs.append(f"{a.name} + {b.name}")
            if missing_pairs:
                print(f"Missing {TIER_LABELS.get(tier, tier)} combos:")
                for pair in missing_pairs:
                    print(f"  - {pair}")
                errors += len(missing_pairs)

    if require_combo_per_element:
        missing_combo_elements: List[str] = []
        for element in elements.values():
            if combo_usage.get(element.guid, 0) == 0:
                missing_combo_elements.append(
                    f"{element.name} ({element.path})"
                )
        if missing_combo_elements:
            print("Elements with no combo entries:")
            for entry in missing_combo_elements:
                print(f"  - {entry}")
            errors += len(missing_combo_elements)

    if require_behaviors:
        missing_behavior_elements: List[str] = []
        for element in elements.values():
            if element.behavior_count <= 0:
                missing_behavior_elements.append(
                    f"{element.name} ({element.path})"
                )
        if missing_behavior_elements:
            print("Elements with no behaviors assigned:")
            for entry in missing_behavior_elements:
                print(f"  - {entry}")
            errors += len(missing_behavior_elements)

    if database_asset is not None:
        if not database_asset.exists():
            print(f"Database asset not found: {database_asset}")
            errors += 1
        else:
            db_elements, db_combos = parse_database_asset(database_asset)

            missing_elements = [
                element for element in elements.values() if element.guid not in db_elements
            ]
            if missing_elements:
                print("Elements missing from database asset:")
                for element in missing_elements:
                    print(f"  - {element.name} ({element.path})")
                errors += len(missing_elements)

            missing_combos = [
                combo for combo in combos if combo.guid not in db_combos
            ]
            if missing_combos:
                print("Combos missing from database asset:")
                for combo in missing_combos:
                    print(f"  - {combo.path}")
                errors += len(missing_combos)

    if require_instant_removal:
        behavior_guids: List[str] = []
        for element in elements.values():
            behavior_guids.extend(parse_behavior_guids(element.path))

        if behavior_guids:
            guid_to_path = build_guid_to_asset_path(assets_root)
            missing_remove_calls: List[str] = []

            for behavior_guid in set(behavior_guids):
                behavior_asset = guid_to_path.get(behavior_guid)
                if behavior_asset is None or not behavior_asset.exists():
                    continue
                script_guid = None
                for line in read_text(behavior_asset).splitlines():
                    if line.strip().startswith("m_Script:"):
                        script_guid = extract_guid_from_line(line)
                        break
                if script_guid is None:
                    continue
                script_path = guid_to_path.get(script_guid)
                if script_path is None or not script_path.exists():
                    continue

                script_text = read_text(script_path)
                if BEHAVIOR_TAGTYPE_NULL_RE.search(script_text) and not BEHAVIOR_REMOVE_CALL_RE.search(script_text):
                    missing_remove_calls.append(str(script_path))

            if missing_remove_calls:
                print("Instantaneous behaviors (TagType null) missing RemoveBehavior call:")
                for entry in sorted(missing_remove_calls):
                    print(f"  - {entry}")
                errors += len(missing_remove_calls)

    return errors


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Check Element and ElementCombo assets for tier consistency."
    )
    parser.add_argument(
        "--assets-root",
        default="Assets",
        help="Path to the Unity Assets folder (default: Assets)",
    )
    parser.add_argument(
        "--allow-missing-pairs",
        action="store_true",
        help="Allow missing same-tier combo assets (default: fail if any are missing).",
    )
    parser.add_argument(
        "--require-combo-per-element",
        action="store_true",
        help="Fail if any element does not appear in at least one combo.",
    )
    parser.add_argument(
        "--require-behaviors",
        action="store_true",
        help="Fail if any element has no behaviors assigned.",
    )
    parser.add_argument(
        "--database-asset",
        default="Assets/Scripts/Elements/Database1.asset",
        help="Path to the ElementDatabase asset used for validation.",
    )
    parser.add_argument(
        "--skip-database-check",
        action="store_true",
        help="Skip validation of elements/combos being referenced by the database asset.",
    )
    parser.add_argument(
        "--require-instant-remove",
        action="store_true",
        help="Fail if a behavior with TagType == null does not call RemoveBehavior().",
    )
    args = parser.parse_args()

    assets_root = Path(args.assets_root).resolve()
    if not assets_root.exists():
        print(f"Assets root not found: {assets_root}")
        return 2

    element_script_guid = find_script_guid(assets_root, "Element.cs")
    combo_script_guid = find_script_guid(assets_root, "ElementCombo.cs")
    if not element_script_guid or not combo_script_guid:
        print("Could not locate Element.cs.meta or ElementCombo.cs.meta.")
        return 2

    elements = build_element_index(assets_root, element_script_guid)
    combos = build_combo_index(assets_root, combo_script_guid)

    if not elements:
        print("No Element assets found. No combos to check.")
        return 0

    require_all_pairs = not args.allow_missing_pairs
    database_asset = None if args.skip_database_check else Path(args.database_asset).resolve()
    errors = check_combos(
        elements,
        combos,
        require_all_pairs,
        args.require_combo_per_element,
        args.require_behaviors,
        database_asset,
        args.require_instant_remove,
        assets_root,
    )
    if errors == 0:
        print("Element combo check passed.")
    else:
        print(f"Element combo check completed with {errors} issue(s).")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
