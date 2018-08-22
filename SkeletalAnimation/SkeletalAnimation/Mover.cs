using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace SkeletalAnimation
{
    /// <summary>
    /// Aparentemente essa classe existe para controlar a animação do carinha
    /// </summary>
    public class Mover : Component
    {
        float MoveSpeed { get; }
        float RotationSpeed { get; }
        BoundingBox Bounds { get; }

        public Mover(float moveSpeed, float rotateSpeed, BoundingBox bounds)
        {
            MoveSpeed = moveSpeed;
            RotationSpeed = rotateSpeed;
            Bounds = bounds;
            ReceiveSceneUpdates = true;
        }

        protected override void OnUpdate(float timeStep)
        {
            // This moves the character position
            Node.Translate(Vector3.UnitZ * MoveSpeed * timeStep, TransformSpace.Local);

            // If in risk of going outside the plane, rotate the model right
            var pos = Node.Position;
            if (pos.X < Bounds.Min.X || pos.X > Bounds.Max.X || pos.Z < Bounds.Min.Z || pos.Z > Bounds.Max.Z)
                Node.Yaw(RotationSpeed * timeStep, TransformSpace.Local);

            // Get the model's first (only) animation
            // state and advance its time. Note the
            // convenience accessor to other components in
            // the same scene node

            var model = GetComponent<AnimatedModel>();
            if (model.NumAnimationStates > 0)
            {
                var state = model.AnimationStates.First();
                state.AddTime(timeStep);
            }
        }
    }
}
