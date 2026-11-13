using System;
using UnityEngine;

namespace LoadingGame
{
    /// <summary>
    /// Data for the modern look of the minigame: palette, shape color families, grade tiers and
    /// animation timings. Panels read it through serialized references.
    /// </summary>
    [CreateAssetMenu(menuName = "Loading/Visual Theme", fileName = "VisualTheme")]
    public class VisualTheme : ScriptableObject
    {
        /// <summary>Fill and target overlay colors for one shape family.</summary>
        [Serializable]
        public struct ColorFamily
        {
            public string label;
            public Color fill;
            public Color target;
        }

        /// <summary>A grade band. A hit falls in the first tier whose maxError is not exceeded.</summary>
        [Serializable]
        public struct GradeTier
        {
            public string label;
            [Tooltip("Inclusive upper bound of |target - hit| for this tier.")]
            public int maxError;
            public Color color;
            public GradeAnimation animation;
            [Tooltip("Entry animation length in seconds.")]
            public float duration;
            [Tooltip("Tints the target tick with the success color instead of the error color.")]
            public bool countsAsGoodHit;
        }

        /// <summary>Durations and amounts for feedback tweens.</summary>
        [Serializable]
        public struct Timings
        {
            public float roundStartPop;
            public float targetPunch;
            public float tapSquashIn;
            public float tapSquashOut;
            public float flash;
            public float buttonAppear;
            public float buttonPress;
            public float tickPulse;
            public float glowTint;
            public float hpCount;
            public float deltaFloat;
            public float levelPunch;
            public float lowHpPulse;
            public float idleDemoFill;
            public float gameOverDelay;
            public float gameOverDim;
            public float gameOverPop;
            public float retryDelay;
            public float trophySpin;
            public float stageShake;
            public float stageShakePixels;
            public float missShake;
            public float missShakePixels;
            public float wobbleDegrees;
        }

        [Header("Sky")]
        public Color skyBottom = Hex(0x24403FFF);
        public Color skyTop = Hex(0x13252AFF);

        [Header("Hairline")]
        [Tooltip("Thin vertical center line behind the shape. Alpha fades toward the top of the screen.")]
        public bool showHairline = true;
        public Color hairline = Hex(0x3F6266FF);

        [Header("Surfaces")]
        public Color surface = Hex(0x1E3438FF);
        public Color raised = Hex(0x27403FFF);
        public Color border = Hex(0x4F777BA0);
        public Color ghost = Hex(0xC8E6E61F);

        [Header("Text")]
        public Color textPrimary = Hex(0xF2FAF9FF);
        public Color textSecondary = Hex(0xADC6C5FF);
        public Color textDisabled = Hex(0x4A5C5DFF);

        [Header("Semantic")]
        public Color accent = Hex(0x4FD1C5FF);
        public Color accentPressed = Hex(0x3BAFA4FF);
        public Color success = Hex(0x5BE38AFF);
        public Color warn = Hex(0xF2B544FF);
        public Color error = Hex(0xFF5C5CFF);

        [Header("Buttons")]
        public Color primaryButtonText = Hex(0x061012FF);
        public Color secondaryButtonText = Hex(0xFF8F8FFF);

        [Header("HUD Icons")]
        public Color hpIcon = Hex(0xFF7A6EFF);
        public Color bestIcon = Hex(0xFFC857FF);
        public Color levelIcon = Hex(0x5EE0D3FF);
        [Tooltip("Opacity of the best-level chip, so it reads as secondary to the current level.")]
        [Range(0f, 1f)] public float bestChipAlpha = 0.85f;

        [Header("Shapes")]
        [Tooltip("Indexed by ShapeColorFamily.")]
        public ColorFamily[] families =
        {
            new ColorFamily { label = "Coral", fill = Hex(0xFF6B5EFF), target = Hex(0xFFC4BCFF) },
            new ColorFamily { label = "Amber", fill = Hex(0xF2B544FF), target = Hex(0xFAE0A8FF) },
            new ColorFamily { label = "Jade", fill = Hex(0x4FE0A0FF), target = Hex(0xBDF5DAFF) },
            new ColorFamily { label = "Cyan", fill = Hex(0x3CC8E8FF), target = Hex(0xB5ECF7FF) },
            new ColorFamily { label = "Periwinkle", fill = Hex(0x8FA8FFFF), target = Hex(0xD3DCFFFF) },
            new ColorFamily { label = "Bone", fill = Hex(0xE6EEEAFF), target = Hex(0xFFFFFFFF) }
        };

        [Range(0f, 1f)] public float targetOverlayAlpha = 0x73 / 255f;
        [Range(0f, 1f)] public float glowAlpha = 0.06f;
        [Tooltip("Multiplier on the corner vignette. Low values keep the bottom of the screen from going black.")]
        [Range(0f, 1f)] public float vignetteStrength = 0.2f;
        [Tooltip("Distance of radial ticks from the shape center.")]
        public float tickRadius = 240f;
        [Tooltip("Ticks for linear fills sit this far outside the shape edge.")]
        public float tickEdgeOffset = 24f;
        [Tooltip("The target tick starts pulsing once the fill is this many percent below the target.")]
        public float tickPulseLead = 15f;
        [Tooltip("HP below this value tints and pulses the HP number.")]
        public int lowHpThreshold = 25;
        [Tooltip("The tap hint is shown up to this level.")]
        public int hintMaxLevel = 2;

        [Header("Grades")]
        public GradeTier[] grades =
        {
            new GradeTier { label = "PERFECT", maxError = 0, color = Hex(0x5BE38AFF), animation = GradeAnimation.PunchWobble, duration = 0.45f, countsAsGoodHit = true },
            new GradeTier { label = "GREAT", maxError = 2, color = Hex(0x4FE0A0FF), animation = GradeAnimation.PopBack, duration = 0.35f, countsAsGoodHit = true },
            new GradeTier { label = "GOOD", maxError = 5, color = Hex(0x4FD1C5FF), animation = GradeAnimation.PopQuad, duration = 0.3f },
            new GradeTier { label = "OK", maxError = 10, color = Hex(0xF2B544FF), animation = GradeAnimation.Fade, duration = 0.25f },
            new GradeTier { label = "MISS", maxError = int.MaxValue, color = Hex(0xFF5C5CFF), animation = GradeAnimation.FadeShake, duration = 0.25f }
        };

        [Header("Timings")]
        public Timings timings = new Timings
        {
            roundStartPop = 0.35f,
            targetPunch = 0.3f,
            tapSquashIn = 0.06f,
            tapSquashOut = 0.25f,
            flash = 0.2f,
            buttonAppear = 0.3f,
            buttonPress = 0.08f,
            tickPulse = 0.25f,
            glowTint = 0.4f,
            hpCount = 0.4f,
            deltaFloat = 0.8f,
            levelPunch = 0.3f,
            lowHpPulse = 0.6f,
            idleDemoFill = 3f,
            gameOverDelay = 0.3f,
            gameOverDim = 0.4f,
            gameOverPop = 0.4f,
            retryDelay = 0.6f,
            trophySpin = 0.5f,
            stageShake = 0.25f,
            stageShakePixels = 8f,
            missShake = 0.3f,
            missShakePixels = 12f,
            wobbleDegrees = 6f
        };

        /// <summary>Returns the family colors for a slot, falling back to the first family if out of range.</summary>
        public ColorFamily GetFamily(ShapeColorFamily family)
        {
            int index = (int)family;
            if (index < 0 || index >= families.Length)
            {
                Debug.LogError($"VisualTheme has no entry for family {family}.", this);
                return families[0];
            }
            return families[index];
        }

        /// <summary>Returns the grade tier for an absolute percentage error.</summary>
        public GradeTier GetGrade(int absoluteError)
        {
            for (int i = 0; i < grades.Length; i++)
            {
                if (absoluteError <= grades[i].maxError)
                {
                    return grades[i];
                }
            }
            return grades[grades.Length - 1];
        }

        /// <summary>Converts 0xRRGGBBAA to a Color.</summary>
        public static Color Hex(uint rgba)
        {
            return new Color32((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)rgba);
        }

        /// <summary>Returns the color with its alpha replaced.</summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
