# Avatar

A round portrait of a participant (in the app bar: an 80px rounded square with 20px corners). Shows a photo (`<img>` inside) when one exists, else the person's initial in `font-display`.

Tones alternate by person so neighbours differ: Justin red, Luana green, Lydia red, Angelika green, Denis red. Lydia and Luana share an L, so Lydia shows "Ly".

Sizes: `--sm` 32px (lists), default 48px (app bar), `--lg` 72px, `--xl` 120px (the reveal result, with `shadow-pop`). Give each avatar `aria-label` or adjacent text with the name.
