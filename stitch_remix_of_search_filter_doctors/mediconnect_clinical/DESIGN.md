# Design System Strategy: The Clinical Atelier

## 1. Overview & Creative North Star
The "Clinical Atelier" is the creative North Star for this design system. We are moving away from the "utility-first" aesthetic of standard medical software and toward a high-end, editorial experience that feels both authoritative and bespoke. 

To break the "template" look, we use **intentional asymmetry** and **tonal depth**. Instead of rigid, centered grids, we utilize wide margins and offset typography to guide the doctor’s eye. This system treats the membership and payment flow not as a transaction, but as an invitation into an elite professional circle. We achieve this through sophisticated layering, high-contrast typography scales, and the total elimination of "boxed-in" UI.

## 2. Colors & Surface Architecture
Our palette is rooted in deep, trust-evoking blues, but its application must be fluid and atmospheric.

### The "No-Line" Rule
**Explicit Instruction:** Designers are prohibited from using 1px solid borders for sectioning or containment. 
Boundaries must be defined solely through background color shifts. For example, a pricing table section should be defined by a `surface-container-low` background sitting against a `surface` page body. Use color transitions, not lines, to indicate where one thought ends and another begins.

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers—like stacked sheets of fine, heavy-stock paper.
*   **Base:** `surface` (#f8f9fa)
*   **Sectioning:** `surface-container-low` (#f3f4f5) for large layout blocks.
*   **Interaction Hubs:** `surface-container-highest` (#e1e3e4) for high-priority utility areas.
*   **Floating Elements:** `surface-container-lowest` (#ffffff) for active cards or payment modals.

### The Glass & Gradient Rule
To provide visual "soul," use subtle linear gradients for primary CTAs: transitioning from `primary` (#00488d) at the top-left to `primary-container` (#005fb8) at the bottom-right. For floating membership benefit tags, utilize **Glassmorphism**: `surface-container-lowest` at 70% opacity with a `24px` backdrop-blur to allow the underlying blue tones to bleed through softly.

## 3. Typography: The Editorial Voice
We pair the architectural precision of **Manrope** with the functional clarity of **Inter**.

*   **Display & Headlines (Manrope):** Use `display-md` for membership tier names and `headline-lg` for value propositions. The wide tracking and geometric builds of Manrope convey a sense of modern prestige.
*   **Titles & Body (Inter):** Use `title-md` for form labels and `body-lg` for plan descriptions. 
*   **Visual Hierarchy:** Create "Editorial Moments" by pairing a `display-sm` price (e.g., "$99") with a `label-sm` interval (e.g., "/ MONTH") positioned with an 8px offset to create an asymmetric, custom-typeset feel.

## 4. Elevation & Depth
Hierarchy is achieved through **Tonal Layering**, mimicking natural light and physical volume.

*   **The Layering Principle:** Place a `surface-container-lowest` card on top of a `surface-container-low` section. This creates a soft, natural "lift" that requires no drop shadow.
*   **Ambient Shadows:** If a card must "float" (e.g., a selected payment method), use an extra-diffused shadow: `0 20px 40px`, 4% opacity, using the `on-surface` color (#191c1d) as the tint. Never use pure black shadows.
*   **The Ghost Border Fallback:** If a border is required for accessibility in input fields, use `outline-variant` (#c2c6d4) at **15% opacity**. 100% opaque borders are strictly forbidden.

## 5. Components

### Pricing Tables (The Signature Component)
*   **Layout:** Avoid three equal columns. Use `12` (3rem) spacing to create a dominant "Recommended" center column that is 10% wider than the others.
*   **Styling:** No dividers. Use a vertical shift in `surface-container` tiers to highlight features. 
*   **Selection:** The active plan should use a `surface-tint` (#005db5) glow rather than a thick border.

### Buttons
*   **Primary:** Gradient of `primary` to `primary-container`. `rounded-lg` (0.5rem). No shadow.
*   **Secondary:** `surface-container-highest` background with `on-primary-fixed-variant` text.
*   **Tertiary:** Ghost style; text-only using `primary` color with a `2.5` (0.625rem) horizontal padding for a wide, premium hit-area.

### Payment Method Selection
*   **Interaction:** Radio buttons are replaced by large-format "Value Tiles." 
*   **State:** Unselected tiles use `surface-container-low`. Selected tiles transition to `surface-container-lowest` with a subtle `primary` inner-glow. 
*   **Icons:** Use `on-secondary-container` for card brand icons to keep the palette sophisticated and muted.

### Input Fields
*   **Style:** Minimalist. No "box." Use a `surface-container-highest` bottom-heavy fill with a `sm` (0.125rem) rounded top. 
*   **Focus:** Transition the background to `primary-fixed` (#d6e3ff) rather than drawing a focus ring.

## 6. Do’s and Don’ts

### Do
*   **Do** use the Spacing Scale `16` (4rem) for top/bottom margins to let the layout breathe like a high-end magazine.
*   **Do** use `tertiary` (#7b3200) sparingly as a "Gold Standard" accent for "Pro" or "Elite" membership badges.
*   **Do** align text-heavy content to a narrow 8-column center track while allowing imagery or cards to bleed into the 12-column grid.

### Don't
*   **Don't** use 1px dividers to separate list items. Use `2` (0.5rem) of vertical whitespace and a subtle background shift instead.
*   **Don't** use high-saturation "Success" greens for "Payment Successful." Use the `primary` blue with a "Success" icon for a more integrated, calm experience.
*   **Don't** use `none` or `sm` roundedness for cards. Stick to `xl` (0.75rem) to maintain the "Soft Minimalism" aesthetic.