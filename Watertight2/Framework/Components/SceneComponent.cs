using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Interfaces;

namespace Watertight.Framework.Components
{
    public class SceneComponent : ActorComponent, ITransformable
    {
        public SceneComponent(Actor Owner)
            :base(Owner)
        {
            
        }

        public SceneComponent()
            : base()
        {
        }

        public SceneComponent Parent
        {
            get;
            set;
        }

        public Vector3 Location
        {
            get;
            set;
        } = Vector3.Zero;
        public Quaternion Rotation
        {
            get;
            set;
        } = Quaternion.Identity;
        public Vector3 Scale
        {
            get;
            set;
        } = Vector3.One;

        public Vector3 GetLocation_WorldSpace()
        {
            Vector3 ParentWS = Parent?.GetLocation_WorldSpace() ?? (Owner?.Location ?? Vector3.Zero);                  
            return ParentWS + Location;
        }

        public Vector3 GetLocation_Relative()
        {
            return Location;
        }

        public Quaternion GetRotation_WorldSpace()
        {
            Quaternion ParentWS = Parent?.GetRotation_WorldSpace() ?? (Owner?.Rotation ?? Quaternion.Identity);
            return ParentWS * Rotation;         
        }

        public Quaternion GetRotation_Relative()
        {
            return Rotation;
        }

        public Vector3 GetScale_WorldSpace()
        {
            Vector3 ParentWS = Parent?.GetScale_WorldSpace() ?? (Owner?.Scale ?? Vector3.One);
            return ParentWS * Scale;
        }

        public Vector3 GetScale_Relative()
        {
            return Scale;
        }
    }
}
