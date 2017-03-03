# csquadtree
This project is a playground for the development of Quadtrees in C#, and more specifically, for use in Unity.

As such, the idea is that you can easily setup a benchmark scenario by choosing a few different Quadtree implementations 
and a point generator, dragging them into a GameObject and pressing play in the Editor.

The point generator should then, on each Update, generate two sets of points: one of positions, and another of seach locations.
For each Quadtree implementation, the position points will be used to build (or rebuild) the respective quadtree. Then the tree 
will be used to find the closest point to each one of the search locations.

Each implementation is timed independently and a nice average is printed to the console.

# Usage
Create an empty scene and, inside an empty GameObject, put a PointGenerator and a few components derived from PointTest.

Press Play and watch the benchmark unfold until the execution times converge. Press Stop. Optimize one of the PointTest
implementations. Repeat.

The project has a scene called TestScene which showcases how to setup a benchmark scenario.

# RandomDeltaPointGenerator
The only point generator implemented so far is the RandomDeltaPointGenerator. It creates "Num Items" spheres inside a 
"Side Length"-sided square on the XY plane. Each frame these transforms are translated in the X and Y directions by a random 
value between -"Max Delta" and "Max Delta", and then restricted to the original square.
The number of search points generated each frame is controlled by the "Num Searches" field.

It was created to simulate the behaviour of characters in a game where the position doesn't change much 
from one frame to the next. Some Quadtrees try to use this assumption to rebuild their trees instead of restarting from scratch.

# PointTest
There are two classes derived from the abstract PointTest: BruteForceTest and IQuadtreeTest. 

The BruteForceTest is used as a reference. The RandomDeltaPointGenerator can use it to check the other implementations' results.

The IQuadtreeTest is a bit more interesting: it's a generic component to test classes implementing the IQuadtree<Component> 
interface. You can drag-and-drop a class file directly into Implementation field and, as long as the class inside has the same
name of the file, it will be instantiated and tested. The rules for constructor selection are:
* Default constructors are accepted;
* Constructors with parameters named p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY, or p_sideLength are accepted;
* The constructor with the most parameters matching the above list will be selected.

I trust the constructor parameter names are self explanatory.

# Current IQuadTree<T> implementations
* Quadtree<T>: This quadtree partitions the area using a random element as a pivot.
* Quadtree2<T>: This implementation (and it's children) statically partitions the area.
* ComponentQuadTree<Component>: Instead of rebuilding the whole tree each frame, this one checks if Component is still inside the same node. If it's not, it's removed and fed at the root again.
* ComponentQuadTree2<Component>: Like the one before, this checks if Component is still inside the same node, but instead of feeding the element back at the top, it tries to climb the tree.

# Important performance criteria
There are three performance axis that interest us: execution time, static memory use and per-frame memory allocation.

At this time, execution time and per-frame memory allocation are the project's main focus. Most trees are rebuilt every frame,
inevitably causing the garbage collector to kick-in. This is an important factor for games and VR applications.
