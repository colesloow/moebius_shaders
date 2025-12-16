# Moebius NPR

This repository contains a Unity project focused on **non-photorealistic rendering (NPR)** inspired by the work of **Moebius**.

The goal is to explore stylized rendering techniques based on **clean line art, screen-space outlines, and hand-drawn shading**, using **Unity URP and Shader Graph**.

## Features

* Screen-space outlines based on depth, normals, and color differences
* Stable outlines on opaque and transparent objects
* Sketchy / hand-drawn line variation using subtle noise
* Screen-space hatching experiments driven by pixel brightness
* Modular Shader Graph setup with custom functions

## Technical Notes

* Built with **Unity URP**
* Shaders are implemented mainly in **Shader Graph**, with a few **Custom Functions (HLSL)** when needed
  
## Project Status

This is an **experimental / exploratory project**.
The focus is on visual research rather than production-ready shaders.

