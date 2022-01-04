For Matt Parker's LED christmas tree project (https://www.youtube.com/watch?v=WuMRJf6B5Q4) I have created an animation showing 2 neutron stars colliding into each other, producing a kilonova. This is a tribute to the first neutron star collision detection, back in 2017. The animation was created in c#, using Rhino7 and Grasshopper. Two spheres mapped in 3D space follow a pre-determined curve approximating a decaying orbit. The two jets exploding from the poles are created with growing cylinders along the z-axis. The final part of the explosion is created by mapping a torus, with both in its major and minor radius gradually expanding.

 Seperate scripts were used to create the CSV files for the decaying orbit and the explosion. The CSV files were then pieced together manually, with added filler frames in between them to add a slight delay.
