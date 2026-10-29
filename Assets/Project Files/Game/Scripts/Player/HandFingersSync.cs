using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Animator))]
    public class HandFingersSync : MonoBehaviour
    {
        private const int PHALANGES_COUNT = 3;

        private readonly static (string side, string finger, HumanBodyBones proximal)[] FOLLOWER_FINGERS =
        {
            ("Left", "Middle", HumanBodyBones.LeftMiddleProximal),
            ("Left", "Ring", HumanBodyBones.LeftRingProximal),
            ("Left", "Little", HumanBodyBones.LeftLittleProximal),
            ("Right", "Middle", HumanBodyBones.RightMiddleProximal),
            ("Right", "Ring", HumanBodyBones.RightRingProximal),
            ("Right", "Little", HumanBodyBones.RightLittleProximal),
        };

        private HumanPoseHandler poseHandler;
        private HumanPose pose;

        private int[] musclePairs;

        private Transform[] preservedBones;
        private Vector3[] preservedPositions;
        private Quaternion[] preservedRotations;

        public static bool IsRequired(Animator animator)
        {
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
                return false;

            foreach (var follower in FOLLOWER_FINGERS)
            {
                if (animator.GetBoneTransform(follower.proximal) != null)
                    return true;
            }

            return false;
        }

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (!IsRequired(animator))
            {
                enabled = false;

                return;
            }

            var pairs = new List<int>();
            var followerBones = new HashSet<HumanBodyBones>();
            foreach (var follower in FOLLOWER_FINGERS)
            {
                if (animator.GetBoneTransform(follower.proximal) == null)
                    continue;

                for (var phalanx = 1; phalanx <= PHALANGES_COUNT; phalanx++)
                {
                    pairs.Add(GetMuscleIndex($"{follower.side} Index {phalanx} Stretched"));
                    pairs.Add(GetMuscleIndex($"{follower.side} {follower.finger} {phalanx} Stretched"));

                    followerBones.Add(follower.proximal + (phalanx - 1));
                }
            }

            musclePairs = pairs.ToArray();

            var bones = new List<Transform>();
            for (HumanBodyBones bone = 0; bone < HumanBodyBones.LastBone; bone++)
            {
                if (followerBones.Contains(bone))
                    continue;

                var boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                    bones.Add(boneTransform);
            }

            preservedBones = bones.ToArray();
            preservedPositions = new Vector3[preservedBones.Length];
            preservedRotations = new Quaternion[preservedBones.Length];

            poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            pose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
        }

        private void LateUpdate()
        {
            for (var i = 0; i < preservedBones.Length; i++)
            {
                preservedBones[i].GetLocalPositionAndRotation(out preservedPositions[i], out preservedRotations[i]);
            }

            poseHandler.GetHumanPose(ref pose);

            for (var i = 0; i < musclePairs.Length; i += 2)
            {
                pose.muscles[musclePairs[i + 1]] = pose.muscles[musclePairs[i]];
            }

            poseHandler.SetHumanPose(ref pose);

            for (var i = 0; i < preservedBones.Length; i++)
            {
                preservedBones[i].SetLocalPositionAndRotation(preservedPositions[i], preservedRotations[i]);
            }
        }

        private void OnDestroy()
        {
            poseHandler?.Dispose();
        }

        private int GetMuscleIndex(string muscleName)
        {
            return System.Array.IndexOf(HumanTrait.MuscleName, muscleName);
        }
    }
}
