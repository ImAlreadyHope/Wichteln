# Regenerates the mascot SVGs: python3 design/mascot.py design/assets/Mascot
"""Generates the original 'Flocke' mascot: a white Persian cat, kawaii cartoon style."""
import math, os, sys

INK = "#3A2A30"
WHITE = "#FFFFFF"
SHADE = "#F3E9EE"      # soft fur shade
PINK = "#F4A6BD"
PINK_D = "#E57D9C"
GREEN = "#1F6B45"
RED = "#C8283C"
RED_D = "#A91F31"
GREEN_D = "#18573A"
EYE = "#2B1F25"
OW = 4.0               # outline width

def fluff(cx, cy, rx, ry, n, r, a0=0, a1=360, jitter=0.0):
    pts = []
    for i in range(n):
        t = math.radians(a0 + (a1 - a0) * i / (n - (0 if a1 - a0 >= 360 else 1)))
        rr = r * (1 + jitter * ((i * 37) % 5 - 2) / 10)
        pts.append((cx + rx * math.cos(t), cy + ry * math.sin(t), rr))
    return pts

def blob(base, bumps, fill, outline=True):
    """base: list of ('ellipse', cx, cy, rx, ry) / ('circle', cx, cy, r); bumps: (x,y,r)"""
    shapes = []
    for b in base:
        shapes.append(b)
    for (x, y, r) in bumps:
        shapes.append(("circle", x, y, r))
    out = []
    if outline:
        for s in shapes:
            out.append(shape(s, INK, OW))
    for s in shapes:
        out.append(shape(s, fill, 0))
    return "\n".join(out)

def shape(s, fill, grow):
    if s[0] == "ellipse":
        _, cx, cy, rx, ry = s
        return f'<ellipse cx="{cx:.1f}" cy="{cy:.1f}" rx="{rx+grow:.1f}" ry="{ry+grow:.1f}" fill="{fill}"/>'
    _, cx, cy, r = s
    return f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r+grow:.1f}" fill="{fill}"/>'

def ear(side):
    # side -1 left, +1 right ; rounded triangle set low and wide (Persian)
    x0 = 120 + side * 44
    tip = (120 + side * 70, 34)
    a = (120 + side * 22, 62)
    b = (120 + side * 82, 80)
    d = f"M{a[0]},{a[1]} Q{tip[0]-side*14},{tip[1]-6} {tip[0]},{tip[1]} Q{tip[0]+side*8},{tip[1]+4} {b[0]},{b[1]} Z"
    inner_a = (120 + side * 34, 64)
    inner_tip = (120 + side * 66, 46)
    inner_b = (120 + side * 72, 76)
    di = f"M{inner_a[0]},{inner_a[1]} Q{inner_tip[0]-side*10},{inner_tip[1]-2} {inner_tip[0]},{inner_tip[1]} Q{inner_tip[0]+side*4},{inner_tip[1]+6} {inner_b[0]},{inner_b[1]} Z"
    return (f'<path d="{d}" fill="{WHITE}" stroke="{INK}" stroke-width="{OW}" stroke-linejoin="round"/>'
            f'<path d="{di}" fill="{PINK}"/>')

def eyes(mode):
    s = []
    for side in (-1, 1):
        cx, cy = 120 + side * 30, 106
        if mode == "sleep":
            s.append(f'<path d="M{cx-13},{cy+2} Q{cx},{cy+12} {cx+13},{cy+2}" fill="none" stroke="{INK}" stroke-width="4.5" stroke-linecap="round"/>')
        elif mode == "wink" and side == 1:
            s.append(f'<path d="M{cx-13},{cy+4} Q{cx},{cy-8} {cx+13},{cy+4}" fill="none" stroke="{INK}" stroke-width="4.5" stroke-linecap="round"/>')
        else:
            s.append(f'<ellipse cx="{cx}" cy="{cy}" rx="17" ry="20" fill="{EYE}"/>')
            s.append(f'<ellipse cx="{cx}" cy="{cy+8}" rx="10" ry="6" fill="#5B4250"/>')
            s.append(f'<circle cx="{cx-5}" cy="{cy-7}" r="6" fill="{WHITE}"/>')
            s.append(f'<circle cx="{cx+6}" cy="{cy+6}" r="2.6" fill="{WHITE}"/>')
    return "\n".join(s)

def face(mode):
    s = [eyes(mode)]
    # blush
    for side in (-1, 1):
        s.append(f'<ellipse cx="{120+side*50}" cy="{124}" rx="11" ry="6.5" fill="{PINK}" opacity=".75"/>')
        # whisker freckles
        for k in range(3):
            s.append(f'<circle cx="{120+side*(54+k*6)}" cy="{112+(k%2)*3}" r="1.4" fill="{INK}" opacity=".35"/>')
    # nose
    s.append(f'<path d="M113,118 Q120,114 127,118 Q124,125 120,126 Q116,125 113,118 Z" fill="{PINK_D}" stroke="{INK}" stroke-width="2" stroke-linejoin="round"/>')
    if mode == "sleep":
        s.append(f'<path d="M112,131 Q116,135 120,131 Q124,135 128,131" fill="none" stroke="{INK}" stroke-width="3" stroke-linecap="round"/>')
    else:
        # open grin with tongue and a tiny fang
        s.append(f'<path d="M108,130 Q120,152 132,130 Q126,134 120,129 Q114,134 108,130 Z" fill="#7A3B4F" stroke="{INK}" stroke-width="2.6" stroke-linejoin="round"/>')
        s.append(f'<path d="M113,140 Q120,135 127,140 Q120,148 113,140 Z" fill="{PINK}"/>')
        s.append(f'<path d="M124,131.5 L127,137 L129.5,131 Z" fill="{WHITE}"/>')
    return "\n".join(s)

def hat():
    # pastel santa hat tipping to the right, pom-pom drooping down
    cone = (f'<path d="M64,58 Q92,2 150,8 Q186,12 206,52 Q190,38 170,40 Q178,54 176,66 Q136,44 64,58 Z" '
            f'fill="{RED}" stroke="{INK}" stroke-width="{OW}" stroke-linejoin="round"/>')
    stripe = f'<path d="M108,20 Q140,10 170,24" fill="none" stroke="{WHITE}" stroke-width="5" stroke-linecap="round" opacity=".6"/>'
    band_pts = []
    for i in range(9):
        t = i / 8
        x = 58 + t * 124
        y = 62 - math.sin(t * math.pi) * 12 + t * 4
        band_pts.append((x, y, 11))
    band = blob([], band_pts, WHITE)
    pom = blob([("circle", 210, 58, 13)], [(202, 50, 7), (219, 52, 7), (214, 67, 7), (203, 66, 6)], WHITE)
    return cone + stripe + band + pom

def scarf():
    s = (f'<path d="M70,160 Q120,184 170,160 L172,176 Q120,200 68,176 Z" fill="{GREEN}" stroke="{INK}" stroke-width="{OW}" stroke-linejoin="round"/>'
         f'<path d="M146,178 L156,218 L174,214 L166,172 Z" fill="{GREEN}" stroke="{INK}" stroke-width="{OW}" stroke-linejoin="round"/>')
    for x in (84, 104, 124, 144):
        s += f'<path d="M{x},{170+abs(x-120)/9:.0f} l6,9" stroke="{WHITE}" stroke-width="4" stroke-linecap="round" opacity=".8"/>'
    s += f'<path d="M152,194 l14,-4 M156,206 l14,-4" stroke="{WHITE}" stroke-width="4" stroke-linecap="round" opacity=".8"/>'
    return s

def body_parts(pose):
    out = []
    # tail: fluffy curve on the right, behind body
    tail = [(176 + 30 * math.sin(t / 10 * 2.2), 214 - t * 7.5, 13 - t * 0.2) for t in range(11)]
    out.append(blob([], tail, WHITE))
    # body
    body_bumps = fluff(120, 186, 58, 44, 18, 13, 150, 390)
    out.append(blob([("ellipse", 120, 188, 60, 46)], body_bumps, WHITE))
    out.append(f'<ellipse cx="120" cy="200" rx="34" ry="24" fill="{SHADE}"/>')
    return "\n".join(out)

def paws(pose):
    out = []
    feet = [(96, 230), (144, 230)]
    for (x, y) in feet:
        out.append(f'<ellipse cx="{x}" cy="{y}" rx="17" ry="11" fill="{WHITE}" stroke="{INK}" stroke-width="{OW}"/>')
        out.append(f'<path d="M{x-5},{y-5} v7 M{x+5},{y-5} v7" stroke="{INK}" stroke-width="2.4" stroke-linecap="round"/>')
    if pose == "gift":
        out.append(gift(120, 196))
        for x in (84, 156):
            out.append(f'<ellipse cx="{x}" cy="190" rx="13" ry="11" fill="{WHITE}" stroke="{INK}" stroke-width="{OW}"/>')
    elif pose == "hello":
        out.append(f'<ellipse cx="96" cy="196" rx="13" ry="11" fill="{WHITE}" stroke="{INK}" stroke-width="{OW}"/>')
        out.append(f'<g transform="rotate(-28 192 140)"><ellipse cx="192" cy="140" rx="14" ry="19" fill="{WHITE}" stroke="{INK}" stroke-width="{OW}"/>'
                   f'<ellipse cx="192" cy="136" rx="6" ry="5" fill="{PINK}"/>'
                   f'<circle cx="184" cy="127" r="3" fill="{PINK}"/><circle cx="192" cy="124" r="3" fill="{PINK}"/><circle cx="200" cy="127" r="3" fill="{PINK}"/></g>')
    else:  # sleep: paws tucked
        for x in (98, 142):
            out.append(f'<ellipse cx="{x}" cy="196" rx="13" ry="10" fill="{WHITE}" stroke="{INK}" stroke-width="{OW}"/>')
    return "\n".join(out)

def gift(cx, cy):
    w, h = 64, 46
    x, y = cx - w / 2, cy - h / 2 + 6
    return (f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="9" fill="{GREEN}" stroke="{INK}" stroke-width="{OW}"/>'
            f'<rect x="{x-4}" y="{y-6}" width="{w+8}" height="16" rx="7" fill="{GREEN}" stroke="{INK}" stroke-width="{OW}"/>'
            f'<rect x="{cx-6}" y="{y-6}" width="12" height="{h+6}" fill="{RED}"/>'
            f'<path d="M{cx},{y-6} q-20,-20 -24,-4 q2,8 24,4 Z M{cx},{y-6} q20,-20 24,-4 q-2,8 -24,4 Z" fill="{RED}" stroke="{INK}" stroke-width="3" stroke-linejoin="round"/>'
            f'<circle cx="{cx}" cy="{y-6}" r="5" fill="{RED_D}" stroke="{INK}" stroke-width="3"/>')

def head():
    bumps = fluff(120, 104, 70, 56, 22, 13, 0, 360)
    # extra cheek fluff tufts
    bumps += [(46, 118, 12), (194, 118, 12), (52, 136, 10), (188, 136, 10)]
    return blob([("ellipse", 120, 104, 74, 58)], bumps, WHITE)

def zzz():
    return (f'<text x="190" y="54" font-family="Fredoka, sans-serif" font-weight="600" font-size="26" fill="{INK}" opacity=".55">z</text>'
            f'<text x="208" y="34" font-family="Fredoka, sans-serif" font-weight="600" font-size="18" fill="{INK}" opacity=".4">z</text>')

def cat(pose="hello", mode=None, with_hat=True):
    mode = mode or {"hello": "open", "gift": "open", "sleep": "sleep", "wink": "wink"}[pose]
    p = "gift" if pose == "wink" else pose
    parts = [body_parts(p), scarf(), ear(-1), ear(1), head(), face(mode)]
    if with_hat:
        parts.append('<g transform="translate(26,-4) rotate(16 130 60) scale(.82)">' + hat() + '</g>')
    parts.append(paws(p))
    if pose == "sleep":
        parts.append(zzz())
    inner = "\n".join(parts)
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 -14 256 262" width="256" height="262" role="img" '
            f'aria-label="Flocke, a white Persian cat">\n<title>Flocke</title>\n{inner}\n</svg>\n')

if __name__ == "__main__":
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    for name, pose in [("flocke-hello", "hello"), ("flocke-gift", "gift"), ("flocke-sleep", "sleep"), ("flocke-wink", "wink")]:
        open(os.path.join(out, name + ".svg"), "w").write(cat(pose))
    print("ok")
