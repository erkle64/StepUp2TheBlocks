using Channel3.ModKit;
using HarmonyLib;
using Unfoundry;
using UnityEngine;

namespace StepUp2TheBlocks
{
    [UnfoundryMod(Plugin.GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "StepUp2TheBlocks",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.1.0";

        public static LogSource log;

        public Plugin()
        {
            log = new LogSource(MODNAME);
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
        }


        [HarmonyPatch]
        public class Patch
        {
            [HarmonyPatch(typeof(RenderCharacter), "handleJumping")]
            [HarmonyPrefix]
            public static bool handleJumping(RenderCharacter __instance, Character.ClientData characterDataToUse, float speedToUse, ref bool __result)
            {
                bool flag1 = false;
                if (__instance.isGrounded || __instance.isSwimming && !__instance.isDiving)
                {
                    __instance.groundedTime += Time.deltaTime;
                    if (__instance.isGrounded)
                        __instance.moveDirection.y = 0.0f;
                    if (__instance.previouslyGrounded)
                        __instance.lastJumpGravityType = RenderCharacter.eJumpGravityType.DEFAULT;
                    if ((double)__instance.groundedTime >= 0.150000005960464 && __instance.inputProxy.getKeyPressed(InputProxy.eKey.JUMP))
                    {
                        flag1 = true;
                        __instance.speedAtJump = speedToUse;
                        __instance.moveDirection.y = 8f;
                        __instance.renderCharacterAnimator.justJumped = true;
                        __instance.lastJumpGravityType = RenderCharacter.eJumpGravityType.DEFAULT;
                        __instance.lastDefaultJumpTime = Time.time;
                        MovementSoundPack packBasedOnPosition = __instance.getMovementSoundPackBasedOnPosition();
                        if (!__instance.audioSource_jump.isPlaying && packBasedOnPosition != (MovementSoundPack)null)
                        {
                            __instance.audioSource_jump.PlayOneShot(packBasedOnPosition.jumpClip_start);
                            if (__instance.audioSource_footsteps.isPlaying)
                                __instance.audioSource_footsteps.Stop();
                        }
                    }
                    Vector3 vector3 = __instance.moveDirection * Time.deltaTime;
                    vector3.y = 0.0f;
                    float maxDistance = Mathf.Max(0.5f, Mathf.Min(1f, vector3.magnitude * 5f));
                    if (characterDataToUse.autoClimbMode == 1 && !characterDataToUse.focusMode())
                    {
                        float a = Mathf.Abs(__instance.moveDirection.x) + Mathf.Abs(__instance.moveDirection.z);
                        if (!flag1 && (double)__instance.groundedTime >= 0.0500000007450581 && !Mathf.Approximately(a, 0.0f))
                        {
                            bool flag2 = Mathf.Approximately(speedToUse, 10f);
                            Vector3 moveDirection = __instance.moveDirection;
                            moveDirection.y = 0.0f;
                            __instance._dbg_sphereCastDirectionAutoClimb = moveDirection.normalized;
                            RenderCharacter.eJumpType eJumpType = RenderCharacter.eJumpType.INVALID;
                            float radius1 = 0.1f;
                            int layerMask1 = GlobalStaticCache.s_LayerMask_Terrain | GlobalStaticCache.s_LayerMask_BuildableObjectPartialSize | GlobalStaticCache.s_LayerMask_BuildableObjectFullSize;
                            int count1 = Physics.SphereCastNonAlloc(new Ray(__instance.transform.position + new Vector3(0.0f, 0.75f, 0.0f), moveDirection.normalized), radius1, __instance.raycastHits_autoClimb, maxDistance, layerMask1, QueryTriggerInteraction.Ignore);
                            if (count1 > 0)
                            {
                                int nearestHit = __instance.raycastHits_autoClimb.findNearestHit(count1);
                                Collider collider = __instance.raycastHits_autoClimb[nearestHit].collider;
                                bool flag3 = !collider.CompareTag("NO_AUTO_CLIMB");
                                if (collider.gameObject.layer == GlobalStaticCache.s_Layer_BuildableObjectFullSize || collider.gameObject.layer == GlobalStaticCache.s_Layer_BuildableObjectPartialSize)
                                {
                                    flag3 = false;
                                    BuildableObjectGO componentInParent = collider.gameObject.GetComponentInParent<BuildableObjectGO>();
                                    if ((UnityEngine.Object)componentInParent != (UnityEngine.Object)null && componentInParent.template != (BuildableObjectTemplate)null && (componentInParent.template.type == BuildableObjectTemplate.BuildableObjectType.Conveyor || componentInParent.template.type == BuildableObjectTemplate.BuildableObjectType.ConveyorBalancer) && (collider.GetType() == typeof(BoxCollider) && !collider.CompareTag("NO_AUTO_CLIMB")))
                                    {
                                        float num = __instance.raycastHits_autoClimb[nearestHit].collider.bounds.max.y - __instance.transform.position.y;
                                        if ((double)num > 0.0 && (double)num < 1.04999995231628)
                                            flag3 = true;
                                    }
                                }
                                if (flag3)
                                    eJumpType = RenderCharacter.eJumpType.FULL_JUMP;
                            }
                            if (eJumpType == RenderCharacter.eJumpType.INVALID)
                            {
                                int count2 = Physics.SphereCastNonAlloc(new Ray(__instance.transform.position + new Vector3(0.0f, 0.25f, 0.0f), moveDirection.normalized), radius1, __instance.raycastHits_autoClimb, maxDistance, layerMask1, QueryTriggerInteraction.Ignore);
                                if (count2 > 0)
                                {
                                    int nearestHit = __instance.raycastHits_autoClimb.findNearestHit(count2);
                                    Collider collider = __instance.raycastHits_autoClimb[nearestHit].collider;
                                    bool flag3 = !collider.CompareTag("NO_AUTO_CLIMB");
                                    if (collider.gameObject.layer == GlobalStaticCache.s_Layer_BuildableObjectFullSize || collider.gameObject.layer == GlobalStaticCache.s_Layer_BuildableObjectPartialSize)
                                    {
                                        BuildableObjectGO componentInParent = collider.gameObject.GetComponentInParent<BuildableObjectGO>();
                                        if ((UnityEngine.Object)componentInParent != (UnityEngine.Object)null && componentInParent.template != (BuildableObjectTemplate)null && (componentInParent.template.type == BuildableObjectTemplate.BuildableObjectType.Conveyor || componentInParent.template.type == BuildableObjectTemplate.BuildableObjectType.ConveyorBalancer))
                                        {
                                            flag3 = false;
                                            if (collider.GetType() == typeof(BoxCollider) && !collider.CompareTag("NO_AUTO_CLIMB"))
                                            {
                                                float num = __instance.raycastHits_autoClimb[nearestHit].collider.bounds.max.y - __instance.transform.position.y;
                                                if ((double)num > 0.0 && (double)num < 1.04999995231628)
                                                    flag3 = true;
                                            }
                                        }
                                    }
                                    if (flag3)
                                        eJumpType = RenderCharacter.eJumpType.HALF_JUMP;
                                }
                            }
                            if (eJumpType != RenderCharacter.eJumpType.INVALID)
                            {
                                float num = eJumpType == RenderCharacter.eJumpType.FULL_JUMP ? 1f : 0.5f;
                                int layerMask2 = GlobalStaticCache.s_LayerMask_Terrain | GlobalStaticCache.s_LayerMask_BuildableObjectFullSize | GlobalStaticCache.s_LayerMask_BuildableObjectPartialSize;
                                float radius2 = 0.3f;
                                Vector3 point0 = __instance.transform.position + new Vector3(0.0f, num + radius2, 0.0f) + moveDirection.normalized * maxDistance;
                                if (Physics.OverlapCapsuleNonAlloc(point0, point0 + new Vector3(0.0f, 1.2f, 0.0f), radius2, __instance.overlapCapsuleResults, layerMask2, QueryTriggerInteraction.Ignore) <= 0)
                                {
                                    __instance.controller.Move(new Vector3(0.0f, eJumpType == RenderCharacter.eJumpType.FULL_JUMP ? 1.2f : 0.7f, 0.0f));
                                }
                            }
                        }
                    }
                }
                if (flag1)
                    __instance.lastJumpTime = Time.time;
                __result = flag1;
                return false;
            }
        }
    }
}
