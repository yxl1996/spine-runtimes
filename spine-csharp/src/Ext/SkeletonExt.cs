namespace Spine
{
    public partial class Skeleton
    {
        public void FullUpdateCache()
        {
            ExposedList<IUpdatable> updateCache = this.updateCache;
            updateCache.Clear();

            int boneCount = this.bones.Count;
            Bone[] bones = this.bones.Items;
            for (int i = 0; i < boneCount; i++)
            {
                Bone bone = bones[i];
                bone.sorted = false;
                bone.active = true;
            }

            int ikCount = this.ikConstraints.Count,
                transformCount = this.transformConstraints.Count,
                pathCount = this.pathConstraints.Count,
                physicsCount = this.physicsConstraints.Count;
            IkConstraint[] ikConstraints = this.ikConstraints.Items;
            TransformConstraint[] transformConstraints = this.transformConstraints.Items;
            PathConstraint[] pathConstraints = this.pathConstraints.Items;
            PhysicsConstraint[] physicsConstraints = this.physicsConstraints.Items;
            int constraintCount = ikCount + transformCount + pathCount + physicsCount;
            for (int i = 0; i < constraintCount; i++)
            {
                for (int ii = 0; ii < ikCount; ii++)
                {
                    IkConstraint constraint = ikConstraints[ii];
                    if (constraint.data.order == i)
                    {
                        SortIkConstraint(constraint);
                        goto continue_outer;
                    }
                }

                for (int ii = 0; ii < transformCount; ii++)
                {
                    TransformConstraint constraint = transformConstraints[ii];
                    if (constraint.data.order == i)
                    {
                        SortTransformConstraint(constraint);
                        goto continue_outer;
                    }
                }

                for (int ii = 0; ii < pathCount; ii++)
                {
                    PathConstraint constraint = pathConstraints[ii];
                    if (constraint.data.order == i)
                    {
                        SortPathConstraint(constraint);
                        goto continue_outer;
                    }
                }

                for (int ii = 0; ii < physicsCount; ii++)
                {
                    PhysicsConstraint constraint = physicsConstraints[ii];
                    if (constraint.data.order == i)
                    {
                        SortPhysicsConstraint(constraint);
                        goto continue_outer;
                    }
                }

                continue_outer:
                {
                }
            }

            for (int i = 0; i < boneCount; i++)
                SortBone(bones[i]);
        }
    }
}