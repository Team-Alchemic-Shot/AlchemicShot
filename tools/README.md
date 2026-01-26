# Element Combo Checker

This folder contains a small utility script that validates element assets and their combo recipes.

## Usage

Run from the repository root:

```
python tools/check_element_combos.py
```

Optional flags:

- `--assets-root Assets` (default)
- `--require-all-pairs` to fail if any same-tier element pair has no combo asset

The script reads Unity `.asset` files, identifies `Element` and `ElementCombo` assets by their script GUIDs, and checks tier consistency.
