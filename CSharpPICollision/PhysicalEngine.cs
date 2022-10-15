using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CSharpPICollision
{
    interface IPhysicalEngine
    {
        /// <summary>
        /// Returns list of physical objects that are contained in PhysicalEngine 
        /// </summary>
        public IReadOnlyList<PhysicalObject> Objects
        {
            get;
        }
        /// <summary>
        /// Delegate that is called after every update
        /// </summary>
        public event EventHandler OnUpdate;
        /// <summary>
        /// Default interval between two frames
        /// </summary>
        public int Interval
        {
            get;
        }

        /// <summary>
        /// Updates properties for all physical objects from the <seealso cref="Objects"/>
        /// </summary>
        public void Update();
        /// <summary>
        /// Sets the time offset to current tick count
        /// </summary>
        public void ResetTime();
    }
    class PhysicalEngine : IPhysicalEngine
    {
        private int _timeUnit
        {
            get => 1000;
        }
        private double _epsilon
        {
            get => 1e-15;
        }

        private List<PhysicalObject> _objects;
        private EventHandler? _updateEvent;
        private double _timeOffset;
        private object _syncObj;

        public IReadOnlyList<PhysicalObject> Objects
        {
            get => _objects;
        }
        public event EventHandler OnUpdate
        {
            add => _updateEvent += value;
            remove => _updateEvent -= value;
        }
        public int Interval
        {
            get => 10;
        }

        /// <summary>
        /// Returns whether two objects are moving towards each other or not
        /// </summary>
        /// <param name="obj1">First object to which method is applied</param>
        /// <param name="obj2">Second object to which method is applied</param>
        /// <returns>
        /// <list>
        /// <item>
        /// <term>False</term>
        /// <description>Objects are moving in opposite directions</description>
        /// </item>
        /// <item>
        /// <term>True</term>
        /// <description>Objects are moving towards each other</description>
        /// </item>
        /// </list>
        /// </returns>
        private bool IsMovingTowards(PhysicalObject obj1, PhysicalObject obj2)
        {
            double distance = obj2.GetPosition() - obj1.GetPosition();
            double relativeVelocity = obj1.GetVelocity() - obj2.GetVelocity();
            int movementDiretion = Math.Abs(distance) > _epsilon ? Math.Sign(distance) : 0;
            double velocityDiretion = Math.Abs(relativeVelocity) > _epsilon ? Math.Sign(relativeVelocity) : 0;

            return movementDiretion == velocityDiretion;
        }
        /// <summary>
        /// Returns time to objects collision
        /// </summary>
        /// <param name="obj1">First object to which method is applied</param>
        /// <param name="obj2">Second object to which method is applied</param>
        /// <returns>Time to collision</returns>
        private double ComputeCollisionTime(PhysicalObject obj1, PhysicalObject obj2)
        {
            double resultVelocity = Math.Abs(obj1.GetVelocity() - obj2.GetVelocity());
            double time = obj1.Distance(obj2) / resultVelocity;

            return time;
        }
        /// <summary>
        /// Returns nearest collision between any two objects in the list
        /// </summary>
        /// <param name="obj1">First object to which method is applied</param>
        /// <param name="obj2">Second object to which method is applied</param>
        /// <returns>Nearest <seealso cref="Collision"/> or null if no objects collide within time delta</returns>
        private Collision? GetNearestCollision(IReadOnlyList<PhysicalObject> objects, double timeDelta)
        {
            Collision? collision = null;

            for (int index1 = 0; index1 < objects.Count; index1++)
            {
                var object1 = objects[index1];

                for (int index2 = 0; index2 < objects.Count; index2++)
                {
                    var object2 = objects[index2];

                    var towards = IsMovingTowards(object1, object2);

                    if (index1 != index2 && towards)
                    {
                        var time = ComputeCollisionTime(object1, object2);

                        if (time < timeDelta && (!collision.HasValue || (time < collision.Value.Time)))
                        {
                            collision = new Collision(object1, object2, time);
                        }
                    }
                }
            }

            return collision;
        }
        /// <summary>
        /// Sets positions of unprocessed objects to their values after time delta
        /// </summary>
        /// <param name="processedObjects">List of processed objects</param>
        /// <param name="timeDelta">Time delta</param>
        private void UpdatePositions(IReadOnlyList<PhysicalObject> processedObjects, double timeDelta)
        {
            foreach (var obj in _objects)
            {
                if (obj is Block && !processedObjects.Contains(obj))
                {
                    var block = (Block)obj;

                    lock (_syncObj)
                    {
                        block.SetPosition(block.GetPosition(timeDelta));
                    }
                }
            }
        }
        /// <summary>
        /// Returns <seealso cref="CollisionResponse"/> of specific target of collision
        /// </summary>
        /// <param name="collision">Collision properties of two object</param>
        /// <param name="target">First or second object</param>
        /// <returns><seealso cref="CollisionResponse"> of specific target</returns>
        /// <exception cref="NotImplementedException">Throws exception if type is not handled</exception>
        private CollisionResponse ComputeCollision(Collision collision, CollisionTarget target)
        {
            return (int)target switch
            {
                0 => new CollisionResponse(collision.Object1.GetPosition(collision.Time), collision.Object1.ProcessCollision(collision.Object2)),
                1 => new CollisionResponse(collision.Object2.GetPosition(collision.Time), collision.Object2.ProcessCollision(collision.Object1)),
                _ => throw new NotImplementedException()
            };
        }
        /// <summary>
        /// Changes properties of all objects that collide on current frame
        /// </summary>
        /// <param name="timeDelta">Time delta between two frames</param>
        /// <returns>List of processed objects</returns>
        private List<PhysicalObject> ProcessCollisions(double timeDelta)
        {
            List<PhysicalObject> processedObjects = new();
            bool computed = false;

            Action<PhysicalObject, CollisionResponse> Respond = (PhysicalObject obj, CollisionResponse response) =>
            {
                if (obj is Block)
                {
                    lock (_syncObj)
                    {
                        ((Block)obj).SetPosition(response.Position);
                        ((Block)obj).SetVelocity(response.Velocity);
                    }
                }
            };

            for (; !computed;)
            {
                var nearestCollision = GetNearestCollision(_objects, timeDelta);

                if (nearestCollision.HasValue)
                {
                    var properties1 = ComputeCollision(nearestCollision.Value, CollisionTarget.First);
                    var properties2 = ComputeCollision(nearestCollision.Value, CollisionTarget.Second);

                    Respond(nearestCollision.Value.Object1, properties1);
                    Respond(nearestCollision.Value.Object2, properties2);

                    processedObjects.AddRange(new[] { nearestCollision.Value.Object1, nearestCollision.Value.Object2 });
                }
                else
                {
                    computed = true;
                }
            }

            return processedObjects;
        }
       
        public void Update()
        {
            var timeDelta = (double)(Environment.TickCount - _timeOffset) / _timeUnit;

            var processedObjects = ProcessCollisions(timeDelta);

            UpdatePositions(processedObjects, timeDelta);

            ResetTime();

            _updateEvent?.Invoke(this, EventArgs.Empty);
        }        
        public void ResetTime()
        {
            _timeOffset = Environment.TickCount;
        }

        /// <summary>
        /// Initilizes new instance of PhysicalEngine
        /// </summary>
        /// <param name="syncObj">Objects that sync physical object compution thread</param>
        /// <param name="objects">Objects owned by PhysicalEngine</param>
        public PhysicalEngine(object syncObj, params PhysicalObject[] objects)
        {
            _objects = new(objects);
            _syncObj = syncObj;

            ResetTime();
        }
    }
}
