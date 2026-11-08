# Button

Buttons start an action. One `wi-btn--primary` per screen at most: on the home screen that is **Zeige meinen Wichtel**.

**Markup:** `<button class="wi-btn wi-btn--primary">` with an optional leading inline icon (`<svg class="wi-icon">`) and a German verb label.

| Variant | Class | Use |
| --- | --- | --- |
| Primary | `wi-btn--primary` | The one main action: reveal, save wishlist. `red-500` fill, `on-red` text. |
| Secondary | `wi-btn--secondary` | Constructive side actions: add a wish. `green-500` fill. |
| Outline | `wi-btn--outline` | Cancel, neutral choices, icon-only actions. |
| Ghost | `wi-btn--ghost` | Low-weight text actions in rows and cards. |

Modifiers: `wi-btn--lg` (56px, the reveal button), `wi-btn--block` (full width on phones), `wi-btn--icon` (40px square; needs `aria-label`).

**Disabled** uses the native `disabled` attribute; it turns to `surface-sunken` with `ink-muted` text. A locked reveal button keeps its place and says when it opens ("Ab 20. November") with the `lock` icon.

Do: keep labels to 1–3 words. Don't: put two primary buttons side by side, or use red text on a red fill.
