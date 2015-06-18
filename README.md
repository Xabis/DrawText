DrawText
==========

A doom builder 2 plugin that creates geometry based on true type fonts.

To use, simply switch to the Draw Text mode and right-click drag to draw the guide line.

This will create a "preview" that can be edited by moving the guide handles around with your mouse.
Hold shift to temporarily invert snapping rules (you will see the guide line change colors)

Open the dock panel to change the text, font, quality, and alignment properties.

There are two drawing modes, where the text can be aligned on a straight line or placed around a circle.

When done, press Enter to create sectors for each letter outline.

Note: Sector creation relies on doom builder itself to make the appropriate line-splitting and vertex-joining decisions.
      This doesnt always work as expected, especially since each character can create a lot of vertices at higher quality ratings.
