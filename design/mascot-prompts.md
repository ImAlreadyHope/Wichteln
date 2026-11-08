# Flocke: image prompts for Gemini

Use these to generate polished illustrations of Flocke that replace the hand-drawn SVGs in `src/Wichteln.Client/wwwroot/img/`.

**How to use them**

1. Run the **character sheet** prompt first, and regenerate until you like Flocke.
2. For every pose, attach that character sheet image to the chat and paste the pose prompt. Gemini then keeps the same cat.
3. Ask for a transparent background. If you get a solid one instead, remove it and export a square PNG, at least 1024 × 1024.
4. Crop to a square and save a 1024px PNG master in `design/assets/Mascot/flocke-<pose>.png` and a 640px WebP in `src/Wichteln.Client/wwwroot/img/flocke-<pose>.webp`. The app already loads those names.

Every prompt begins with the same style block so the poses match.

---

## Style block (already included in every prompt below)

> Cute kawaii mascot illustration of "Flocke", an original Siamese cat character. Big round head, soft cream-white short coat (#FBF3E8) with classic Siamese seal-brown points (#6B4A3A) on the large pointed ears, the face mask around the muzzle, the paws and the slim tail. Huge glossy sapphire-blue eyes with dark pupils and two white highlights each, pink oval blush on the cheeks, a tiny dark nose, and a cheeky open grin showing one tiny fang. Pink inner ears, chubby round body, short stubby paws. Wears a red Santa hat (#C8283C) with a white fluffy brim and pom-pom, tilted to one side, and a pine-green knitted scarf (#1F6B45) with white stripes. Playful, mischievous, expressive cartoon energy with big emotions and squashy shapes. Clean flat vector sticker style with a consistent thick dark plum-brown outline (#2E2224), soft cel shading in light beige on the cream fur, no gradients, no texture. Centered full body, generous padding, transparent background, no text, no logo, no shadow on the ground.

---

## 1. Character sheet (do this first)

```
Cute kawaii mascot illustration of "Flocke", an original Siamese cat character. Big round head, soft cream-white short coat (#FBF3E8) with classic Siamese seal-brown points (#6B4A3A) on the large pointed ears, the face mask around the muzzle, the paws and the slim tail. Huge glossy sapphire-blue eyes with dark pupils and two white highlights each, pink oval blush on the cheeks, a tiny dark nose, and a cheeky open grin showing one tiny fang. Pink inner ears, chubby round body, short stubby paws. Wears a red Santa hat (#C8283C) with a white fluffy brim and pom-pom, tilted to one side, and a pine-green knitted scarf (#1F6B45) with white stripes. Playful, mischievous, expressive cartoon energy with big emotions and squashy shapes. Clean flat vector sticker style with a consistent thick dark plum-brown outline (#2E2224), soft cel shading in light beige on the cream fur, no gradients, no texture. Transparent background, no text, no logo.

Character turnaround sheet: front view, three-quarter view and side view of Flocke standing on all fours, plus a close-up of the face, evenly spaced in one row on a plain white background.
```

## 2. Hello (`flocke-hello`): login and welcome screen

```
[style block]

Pose: Flocke sits upright facing the viewer and waves one raised front paw high next to her face, showing the pink paw pads. Happy, excited expression with a wide open grin and sparkling eyes, head tilted slightly. The other paw rests on her tummy.
```

## 3. Sleep (`flocke-sleep`): locked draw before 20 November

```
[style block]

Pose: Flocke sits curled up and dozing, eyes closed as two gentle upward-curved lines, peaceful tiny smile with the mouth closed, front paws tucked in, tail wrapped around the body. The Santa hat has slipped slightly over one ear. Two small "z z" shapes float above the hat in the outline color.
```

## 4. Gift (`flocke-gift`): unlocked draw and empty wishlists

```
[style block]

Pose: Flocke sits facing the viewer and proudly holds up a square gift box with both front paws in front of her chest. The box is pine green (#1F6B45) with a red ribbon (#C8283C) and a big red bow on top. Excited open grin with the tiny fang, eyes wide and sparkling.
```

## 5. Wink (`flocke-wink`): next to the revealed Wichtel

```
[style block]

Pose: Flocke sits holding the same green gift box with the red bow, leans to one side and winks with her right eye (a curved line) while the left eye stays big and glossy. Sly, playful "it's a secret" grin. One paw raises a single claw to her lips in a "psst" gesture.
```

## 6. Celebrate (optional): the moment the reel lands

```
[style block]

Pose: Flocke jumps in the air with all four paws spread out, mouth wide open in a joyful cheer, eyes squeezed shut in happiness, the scarf flying up behind her. Small gold stars (#D9A441) and red and green confetti pop around her.
```

## 7. Thinking (optional): loading screen

```
[style block]

Pose: Flocke sits with one paw on her chin and looks up and to the side, curious and thinking. Eyes big and glossy, mouth a small "o". A small gold star (#D9A441) sparkles above her head.
```

---

**Tips**

- If the cat comes out too realistic, add: *"simple shapes, chibi proportions, head as big as the body"*.
- If the points come out grey or blurry, add: *"crisp seal-brown Siamese points with a clean edge, no colour gradient"*.
- If the outline varies in thickness, add: *"uniform 6px outline, sticker die-cut look"*.
- Keep the same chat open for every pose so Gemini keeps the same character.
