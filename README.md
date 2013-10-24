Universe
========

Kinect application that allows to explore different planet systems, like our solar system.

Scene Files
===========

The scene files need to be of following schema:

<?xml version="1.0"?>
<Planets>
  <[Planet Name]>
    <Name></Name>
    <Description></Description>
    <Model>[The 3d model to use]</Model>
    <Rotation_X></Rotation_X>
    <Rotation_Y></Rotation_Y>
    <Rotation_Z></Rotation_Z>
    <Position_X></Position_X>
    <Position_Y></Position_Y>
    <Position_Z></Position_Z>
    <Texture>[Texture to use]</Texture>
    <Speed>[Velocity]</Speed>
    <Radious>[Radius]</Radious>
    <Size>[Size relative to earth]</Size>
    <Update>[Weather need position update]</Update>
  </[Planet Name]>
</Planets>
