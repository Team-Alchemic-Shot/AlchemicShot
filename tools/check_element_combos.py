#!/usr/bin/env python3
"""Check Element assets and ElementCombo assets for tier-consistent combos.

Scans Unity .asset files, identifies Element and ElementCombo assets by matching
m_Script GUIDs, and validates:
- elements are in folders matching their tier (Primitive/Tier1/Tier2)
- combos use valid element references
- combos within the same tier produce the next tier
- missing combos within the same tier (optional)
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


def parse_element_asset(path: Path) -> Tuple[Optional[str], Optional[int]]:
    name: Optional[str] = None
    tier: Optional[int] = None
    for line in read_text(path).splitlines():
        if name is None:
            name_match = ELEMENT_NAME_RE.match(line)
            if name_match:
                name = name_match.group(1).strip().strip('"')
        if tier is None:
            tier_match = ELEMENT_TIER_RE.match(line)
            if tier_match:
                tier = int(tier_match.group(1))
        if name is not None and tier is not None:
            break
    return name, tier


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

        name, tier = parse_element_asset(asset_path)
        name = name or asset_path.stem
        elements[asset_path.as_posix()] = ElementInfo(
            guid=asset_path.as_posix(),
            name=name,
            tier=tier,
            path=asset_path,
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


def unordered_pair(a: str, b: str) -> Tuple[str, str]:
    return (a, b) if a < b else (b, a)


def check_combos(
    elements: Dict[str, ElementInfo],
    combos: List[ComboInfo],
    require_all_pairs: bool,
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
    for combo in combos:
        if not combo.input_a or not combo.input_b:
            print(f"Combo {combo.path} missing input element references.")
            errors += 1
            continue
        combo_lookup[unordered_pair(combo.input_a, combo.input_b)] = combo

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
                for j in range(i + 1, len(tier_elements)):
                    a = tier_elements[i]
                    b = tier_elements[j]
                    if unordered_pair(a.guid, b.guid) not in combo_lookup:
                        missing_pairs.append(f"{a.name} + {b.name}")
            if missing_pairs:
                print(f"Missing {TIER_LABELS.get(tier, tier)} combos:")
                for pair in missing_pairs:
                    print(f"  - {pair}")
                errors += len(missing_pairs)

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
    errors = check_combos(elements, combos, require_all_pairs)
    if errors == 0:
        print("Element combo check passed.")
    else:
        print(f"Element combo check completed with {errors} issue(s).")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
