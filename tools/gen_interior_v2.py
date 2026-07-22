"""
Generate warm-lit interior sprite for the building windows.
Strategy: For each row, find the leftmost/rightmost opaque pixel in the base image.
Pixels between them that are transparent = windows. Fill those with warm glow.
"""
from PIL import Image
import numpy as np

BASE_PATH = "E:/测试1号/SBPK/Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png"
OUTPUT_PATH = "E:/测试1号/SBPK/Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_interior.png"

img = Image.open(BASE_PATH)
arr = np.array(img)
h, w = arr.shape[:2]
alpha = arr[:, :, 3]

# Per-row building extent: leftmost & rightmost opaque pixel
building_left = np.full(h, -1, dtype=int)
building_right = np.full(h, -1, dtype=int)

for y in range(h):
    row_alpha = alpha[y]
    opaque = np.where(row_alpha > 20)[0]
    if len(opaque) > 0:
        building_left[y] = opaque[0]
        building_right[y] = opaque[-1]

# Find global building y range (where at least some opaque pixels exist)
valid_ys = np.where(building_left >= 0)[0]
y_min, y_max = valid_ys[0], valid_ys[-1]
print(f"Building y range: {y_min} - {y_max}")

# Build the interior sprite
interior = np.zeros((h, w, 4), dtype=np.uint8)

# Warm color palette
warm_palette = [
    (255, 200, 100),   # Warm gold
    (240, 170, 80),    # Amber
    (220, 140, 60),    # Deep amber
    (250, 220, 130),   # Light golden
    (200, 130, 70),    # Warm brown
    (230, 160, 90),    # Peach
    (255, 180, 90),    # Bright orange
    (210, 150, 100),   # Warm wood
]

np.random.seed(42)  # reproducible

for y in range(y_min, y_max + 1):
    L = building_left[y]
    R = building_right[y]
    if L < 0 or R < 0:
        continue

    # Vertical brightness gradient (top = brighter, bottom = dimmer)
    t = (y - y_min) / max(1, y_max - y_min)
    vertical_brightness = 0.7 + 0.3 * (1 - t)

    for x in range(L, R + 1):
        if alpha[y, x] < 20:  # window pixel
            # Room seed for color variation
            room_seed = (x // 18 + y // 14)
            color_idx = abs(hash((x // 20, y // 18))) % len(warm_palette)
            base_color = warm_palette[color_idx]

            # Random per-room brightness variation
            room_brightness = 0.6 + 0.4 * (abs(hash((x // 30, y // 24))) % 100 / 100.0)
            brightness = vertical_brightness * room_brightness

            r = int(base_color[0] * brightness)
            g = int(base_color[1] * brightness)
            b = int(base_color[2] * brightness)

            # Floor divider: every 14-15 rows
            local_y = y - y_min
            is_floor_divider = (local_y % 16) < 2

            # Room divider: every 20 cols
            local_x = x - L
            is_room_divider = (local_x % 22) < 2

            if is_floor_divider or is_room_divider:
                # Darker at dividers (window frame, room walls)
                r = max(0, r - 60)
                g = max(0, g - 40)
                b = max(0, b - 20)
                alpha_val = 230
            else:
                alpha_val = 200

            # Add some noise for depth
            noise = np.random.randint(-12, 13)
            r = max(0, min(255, r + noise))
            g = max(0, min(255, g + noise))
            b = max(0, min(255, b + noise))

            interior[y, x] = (r, g, b, alpha_val)

# Save
interior_img = Image.fromarray(interior, 'RGBA')
interior_img.save(OUTPUT_PATH)

# Stats
opx = np.sum(interior[:, :, 3] > 0)
print(f"Saved: {OUTPUT_PATH}")
print(f"Filled pixels: {opx} / {h*w} ({opx/(h*w)*100:.1f}%)")
print(f"Filled within building footprint: {opx / (h*w):.1%} of canvas")
