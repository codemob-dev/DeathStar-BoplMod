using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Assertions;

namespace DeathStar
{
    [BepInPlugin("com.codemob.deathstar", "Death Star", "1.0.0")]
    public class DeathStar : BaseUnityPlugin
    {
        public Harmony harmony;
        private void Awake()
        {
            harmony = new Harmony(Info.Metadata.GUID);
            harmony.PatchAll(typeof(DeathStar));

            MethodInfo beam_UpdateBeam = typeof(Beam).GetMethod("UpdateBeam", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo beam_UpdateBeam_Postfix = typeof(DeathStar).GetMethod(nameof(Beam_UpdateBeam_Postfix), BindingFlags.Public | BindingFlags.Static);
            MethodInfo beam_UpdateBeam_Prefix = typeof(DeathStar).GetMethod(nameof(Beam_UpdateBeam_Prefix), BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(beam_UpdateBeam, 
                prefix: new HarmonyMethod(beam_UpdateBeam_Prefix),
                postfix: new HarmonyMethod(beam_UpdateBeam_Postfix));

            MethodInfo beam_MovePlayer = typeof(Beam).GetMethod("MovePlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo beam_MovePlayer_Patch = typeof(DeathStar).GetMethod(nameof(Beam_MovePlayer), BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(beam_MovePlayer,
                postfix: new HarmonyMethod(beam_MovePlayer_Patch));

            
        }
        [HarmonyPatch(typeof(DetPhysics), nameof(DetPhysics.AddBeamBody))]
        [HarmonyPrefix]
        public static void DetPhysics_AddBeamBody(ref DetPhysics.BeamBody body)
        {
            body.scale = (Fix)4;
        }

        public static void Beam_UpdateBeam_Postfix(ref PlayerBody ___body, ref Vec2 ___staffDir, ref PlayerPhysics ___physics)
        {
            ___body.AddForce(-___staffDir * (Fix).075);
        }

        public static void Beam_UpdateBeam_Prefix(ref Beam __instance)
        {
            __instance.angularAimSpeed = (Fix).03;
        }

        public static void Beam_MovePlayer(ref PlayerPhysics ___physics, ref bool ___inAir)
        {
            if (___physics.IsGrounded() || ___inAir)
                ___physics.AddGravityFactor();
            if (___physics.IsGrounded())
            {
                ___physics.UnGround();
            }
        }

        [HarmonyPatch(typeof(Beam), nameof(Beam.ExitAbility), typeof(AbilityExitInfo))]
        [HarmonyPrefix]
        public static bool Beam_ExitAbility(ref AbilityExitInfo exitInfo)
        {
            return !exitInfo.justlanded;
        }

        [HarmonyPatch(typeof(Beam), nameof(Beam.UpdateSim))]
        [HarmonyPrefix]
        public static void Beam_UpdateSim(ref Beam __instance, ref bool ___inAir)
        {
            ___inAir = true;
        }
    }
}
