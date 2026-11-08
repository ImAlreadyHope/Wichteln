# TextField

A labelled single-line input. Wrap in `wi-field`; give it a `wi-label` bound by `for`/`id`, the `wi-input`, and optionally a `wi-help` line.

Errors: set `aria-invalid="true"` on the input and swap the helper to `wi-help--error` with a sentence that says how to fix it. Focus shows a `ring` border plus a 3px `red-100` halo.

Inputs are 44px tall, 16px text, so iOS does not zoom on focus.
