# Element Combo Checker

This folder contains a small utility script that validates element assets and their combo recipes.

## Usage

Run from the repository root:

```
python tools/check_element_combos.py
```

Optional flags:

- `--assets-root Assets` (default)
- `--allow-missing-pairs` to allow missing same-tier combo assets (default is strict)
- `--max-tier 1` to only validate up to a maximum element tier (e.g. Primitive/Tier1)
- `--require-combo-per-element` to fail if any element does not appear in at least one combo
- `--require-behaviors` to fail if any element has no behaviors assigned
- `--database-asset Assets/Scripts/Elements/Database1.asset` to set the ElementDatabase asset path
- `--skip-database-check` to skip validating that all elements/combos are referenced by the database asset
- `--require-instant-remove` to fail if any behavior with `TagType == null` does not call `RemoveBehavior()`

Recommended command:
`python tools/check_element_combos.py --max-tier 1 --require-combo-per-element --require-behavior --require-instant-remove`

The script reads Unity `.asset` files, identifies `Element` and `ElementCombo` assets by their script GUIDs, and checks tier consistency.
