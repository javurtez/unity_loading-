using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LoadingGame.EditorTools
{
    /// <summary>
    /// Applies the modern visual theme to the sky material, the shape slot prefabs and both panels.
    /// Every step finds or creates objects by name, so running it again yields the same result.
    /// </summary>
    public static class ModernThemeApplier
    {
        private const string ThemePath = "Assets/Scriptable/VisualTheme.asset";
        private const string SkyPath = "Assets/Shader/Sky.mat";
        private const string SlotFolder = "Assets/Prefabs";
        private const string LoadPanelPath = "Assets/Resources/Panels/LoadPanel.prefab";
        private const string ScorePanelPath = "Assets/Resources/Panels/ScorePanel.prefab";
        private const float HairlineWidth = 2f;
        private const string HpIconPath = "Assets/Sprites/UI/heart-plus.png";
        private const string LevelIconPath = "Assets/Sprites/UI/stairs.png";
        private const string BestIconPath = "Assets/Sprites/UI/trophy.png";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string WhiteSpritePath = "Assets/Sprites/UI/GUI_White.png";
        private const string FontMaterialFolder = "Assets/Fonts";
        private const string RegularMaterialPath = FontMaterialFolder + "/LiberationSans SDF - LS_Regular.mat";
        private const string HeavyMaterialPath = FontMaterialFolder + "/LiberationSans SDF - LS_Heavy.mat";

        private const float ShapeSize = 440f;
        private const float ShapeY = 120f;
        private const float ButtonBottomY = 231f;
        private const float PillTextureSize = 128f;

        private static readonly Dictionary<string, ShapeColorFamily> SlotFamilies = new Dictionary<string, ShapeColorFamily>
        {
            { "Hearts", ShapeColorFamily.Rose }, { "Mask", ShapeColorFamily.Rose }, { "Scorpion", ShapeColorFamily.Rose },
            { "Spades", ShapeColorFamily.Amber }, { "Taurus", ShapeColorFamily.Amber }, { "Police Badge", ShapeColorFamily.Amber },
            { "Clover", ShapeColorFamily.Mint }, { "Tear", ShapeColorFamily.Mint }, { "Gemini", ShapeColorFamily.Mint },
            { "Aquarius", ShapeColorFamily.Sky }, { "Diamonds", ShapeColorFamily.Sky }, { "Plain Arrow", ShapeColorFamily.Sky },
            { "Hexagon", ShapeColorFamily.Violet }, { "Polygon", ShapeColorFamily.Violet }, { "Box 1", ShapeColorFamily.Violet },
            { "Egg", ShapeColorFamily.Pearl }, { "Onigiri", ShapeColorFamily.Pearl }, { "Box 2", ShapeColorFamily.Pearl }
        };

        private static VisualTheme theme;
        private static TMP_FontAsset font;
        private static Material regular;
        private static Material heavy;
        private static Sprite pill;
        private static Sprite ring;
        private static Sprite glow;
        private static Sprite vignette;
        private static Sprite white;

        /// <summary>Runs every theme step.</summary>
        [MenuItem("Tools/Loading/Apply Modern Theme")]
        public static void Apply()
        {
            ProceduralSprites.GenerateAll();
            LoadAssets();
            ApplySky();
            ApplySlots();
            ApplyLoadPanel();
            ApplyScorePanel();
            AssetDatabase.SaveAssets();
            Debug.Log("Modern theme applied.");
        }

        private static void LoadAssets()
        {
            theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<VisualTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            ProceduralSprites.EnsureFolder(FontMaterialFolder);
            regular = EnsureFontMaterial(RegularMaterialPath, false);
            heavy = EnsureFontMaterial(HeavyMaterialPath, true);

            pill = ProceduralSprites.Load(ProceduralSprites.PillPath);
            ring = ProceduralSprites.Load(ProceduralSprites.RingPath);
            glow = ProceduralSprites.Load(ProceduralSprites.GlowPath);
            vignette = ProceduralSprites.Load(ProceduralSprites.VignettePath);
            white = ProceduralSprites.Load(WhiteSpritePath);
        }

        private static Material EnsureFontMaterial(string path, bool isHeavy)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material);
                AssetDatabase.CreateAsset(material, path);
            }
            material.CopyPropertiesFromMaterial(font.material);
            material.shaderKeywords = font.material.shaderKeywords;
            material.SetFloat(ShaderUtilities.ID_FaceDilate, isHeavy ? 0.15f : 0f);
            if (isHeavy)
            {
                material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                material.SetColor(ShaderUtilities.ID_UnderlayColor, VisualTheme.Hex(0x00000066));
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
            }
            else
            {
                material.DisableKeyword(ShaderUtilities.Keyword_Underlay);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplySky()
        {
            var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyPath);
            Undo.RecordObject(sky, "Apply Modern Theme");
            sky.SetColor("_Color1", theme.skyBottom);
            sky.SetColor("_Color2", theme.skyTop);
            // The original material pointed _UpVector down, which drew skyTop at the bottom of the screen.
            sky.SetVector("_UpVector", new Vector4(0f, 1f, 0f, 0f));
            sky.SetFloat("_UpVectorPitch", 0f);
            sky.SetFloat("_UpVectorYaw", 0f);
            sky.SetFloat("_Exponent", 1f);
            sky.SetFloat("_Intensity", 1f);
            EditorUtility.SetDirty(sky);
        }

        private static void ApplySlots()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { SlotFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                var slot = root.GetComponent<LoadSlot>();
                if (slot != null && SlotFamilies.TryGetValue(root.name, out ShapeColorFamily family))
                {
                    StyleSlot(slot, family);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                else if (slot != null)
                {
                    Debug.LogWarning("No color family mapped for slot prefab " + path);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void StyleSlot(LoadSlot slot, ShapeColorFamily family)
        {
            var rect = (RectTransform)slot.transform;
            rect.sizeDelta = new Vector2(ShapeSize, ShapeSize);
            rect.anchoredPosition = new Vector2(0f, ShapeY);

            VisualTheme.ColorFamily colors = theme.GetFamily(family);
            slot.loadPercentageImage.color = VisualTheme.WithAlpha(colors.fill, 1f);
            slot.correctPercentageImage.color = VisualTheme.WithAlpha(colors.target, theme.targetOverlayAlpha);

            var ghost = slot.loadPercentageImage.transform.parent.GetComponent<Image>();
            if (ghost != null) ghost.color = theme.ghost;

            Transform fill = slot.loadPercentageImage.transform;
            Transform correct = slot.correctPercentageImage.transform;
            if (fill.GetSiblingIndex() > correct.GetSiblingIndex()) fill.SetSiblingIndex(correct.GetSiblingIndex());

            var group = Ensure<CanvasGroup>(slot.gameObject);
            group.blocksRaycasts = false;
            SetFields(slot, ("colorFamily", (int)family), ("canvasGroup", group));
        }

        private static void ApplyLoadPanel()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(LoadPanelPath);
            var panel = root.GetComponent<LoadPanel>();
            Transform t = root.transform;

            Image hairline = BuildHairline(t);
            Image glowImage = BuildGlow(t);
            RectTransform stage = BuildStage(t);
            RectTransform overlay = Child(t, "ShapeOverlay");
            Layout(overlay, Center, Center, new Vector2(0f, ShapeY), new Vector2(ShapeSize, ShapeSize));
            Image flashImage = BuildFlash(overlay);
            PercentTick targetTick = BuildTick(overlay, "TargetTick");
            PercentTick hitTick = BuildTick(overlay, "HitTick");
            CanvasGroup title = BuildTitle(t);
            RectTransform number = StyleTarget(panel);
            CanvasGroup hint = BuildHint(t);
            HitRow row = StyleHitRow(panel);
            GradePresenter grade = BuildGrade(t, stage.GetComponent<UiShake>());
            GameOverPresenter gameOver = BuildGameOver(t);

            StylePrimaryButton(panel.playButton);
            StylePrimaryButton(panel.nextButton);
            StyleSecondaryButton(panel.retryButton);

            Order(hairline.transform, glowImage.transform, stage, overlay, title.transform, panel.target.transform, hint.transform,
                panel.hit.transform, grade.transform, gameOver.transform,
                panel.nextButton.transform, panel.retryButton.transform, panel.playButton.transform);

            var view = Ensure<LoadPanelView>(root);
            SetFields(view,
                ("theme", theme), ("titleGroup", title), ("hairlineGroup", hairline.GetComponent<CanvasGroup>()),
                ("glowImage", glowImage),
                ("targetTick", targetTick), ("hitTick", hitTick),
                ("flashGroup", flashImage.GetComponent<CanvasGroup>()), ("flashImage", flashImage),
                ("targetNumber", number), ("hintGroup", hint),
                ("hitRowGroup", row.group), ("fromText", row.from), ("deltaText", row.delta),
                ("grade", grade), ("gameOver", gameOver));
            SetFields(panel, ("slotContainer", stage), ("view", view), ("theme", theme));

            PrefabUtility.SaveAsPrefabAsset(root, LoadPanelPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static Image BuildHairline(Transform parent)
        {
            RectTransform rect = Child(parent, "Hairline");
            Layout(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(HairlineWidth, 0f));
            Image image = SetImage(rect, ProceduralSprites.Load(ProceduralSprites.HairlinePath), theme.hairline);
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.alpha = 1f;
            group.blocksRaycasts = false;
            rect.gameObject.SetActive(theme.showHairline);
            return image;
        }

        private static Image BuildGlow(Transform parent)
        {
            RectTransform rect = Child(parent, "Glow");
            Layout(rect, Center, Center, new Vector2(0f, ShapeY), new Vector2(900f, 900f));
            Image image = SetImage(rect, glow, VisualTheme.WithAlpha(theme.GetFamily(ShapeColorFamily.Violet).fill, theme.glowAlpha));
            return image;
        }

        private static RectTransform BuildStage(Transform parent)
        {
            RectTransform rect = Child(parent, "ShapeStage");
            Stretch(rect);
            var shake = Ensure<UiShake>(rect.gameObject);
            SetFields(shake, ("target", rect));
            return rect;
        }

        private static Image BuildFlash(RectTransform overlay)
        {
            RectTransform rect = Child(overlay, "Flash");
            Stretch(rect);
            Image image = SetImage(rect, white, Color.white);
            image.type = Image.Type.Simple;
            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            return image;
        }

        private static PercentTick BuildTick(RectTransform overlay, string name)
        {
            RectTransform pivot = Child(overlay, name);
            Layout(pivot, Center, Center, Vector2.zero, Vector2.zero);
            var group = Ensure<CanvasGroup>(pivot.gameObject);
            group.alpha = 0f;
            group.blocksRaycasts = false;

            RectTransform mark = Child(pivot, "Mark");
            Layout(mark, Center, Center, new Vector2(0f, -theme.tickRadius), new Vector2(6f, 40f));
            Image markImage = SetImage(mark, white, Color.white);

            var tick = Ensure<PercentTick>(pivot.gameObject);
            SetFields(tick, ("pivot", pivot), ("mark", mark), ("markImage", markImage), ("canvasGroup", group));
            return tick;
        }

        private static CanvasGroup BuildTitle(Transform parent)
        {
            RectTransform rect = Child(parent, "Title");
            Layout(rect, Center, Center, new Vector2(0f, 470f), new Vector2(760f, 200f));
            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.blocksRaycasts = false;

            Text(Child(rect, "TitleText"), "LOADING-", 64f, heavy, theme.textPrimary, 4f, new Vector2(0f, 20f), new Vector2(760f, 80f));
            Text(Child(rect, "Tagline"), "Stop at the shown percentage!", 28f, regular, theme.textSecondary, 0f, new Vector2(0f, -50f), new Vector2(760f, 40f));
            return group;
        }

        private static RectTransform StyleTarget(LoadPanel panel)
        {
            var rect = (RectTransform)panel.target.transform;
            Layout(rect, Center, Center, new Vector2(0f, 470f), new Vector2(600f, 200f));

            TextMeshProUGUI caption = FirstOtherText(rect, panel.targetPercentageText);
            if (caption != null)
            {
                Text((RectTransform)caption.transform, "TARGET", 30f, regular, VisualTheme.WithAlpha(theme.textSecondary, 0.8f), 12f, new Vector2(0f, 35f), new Vector2(600f, 40f));
                caption.fontStyle = FontStyles.UpperCase;
            }
            TextMeshProUGUI number = panel.targetPercentageText;
            Text((RectTransform)number.transform, "<mspace=0.6em>62</mspace><size=60%>%</size>", 96f, heavy, theme.textPrimary, 0f, new Vector2(0f, -40f), new Vector2(600f, 110f));
            return number.rectTransform;
        }

        private static CanvasGroup BuildHint(Transform parent)
        {
            RectTransform rect = Child(parent, "Hint");
            Text(rect, "TAP TO STOP", 24f, regular, theme.textPrimary, 12f, new Vector2(0f, -300f), new Vector2(600f, 40f));
            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            return group;
        }

        private struct HitRow
        {
            public CanvasGroup group;
            public TextMeshProUGUI from;
            public TextMeshProUGUI delta;
        }

        private static HitRow StyleHitRow(LoadPanel panel)
        {
            var rect = (RectTransform)panel.hit.transform;
            Layout(rect, Center, Center, new Vector2(0f, -295f), new Vector2(400f, 50f));

            var layout = Ensure<HorizontalLayoutGroup>(rect.gameObject);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var fitter = Ensure<ContentSizeFitter>(rect.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.alpha = 0f;
            group.blocksRaycasts = false;

            TextMeshProUGUI hitText = panel.hitPercentageText;
            TextMeshProUGUI from = FirstOtherText(rect, hitText, "Arrow", "Delta");
            if (from == null) from = Ensure<TextMeshProUGUI>(Child(rect, "From").gameObject);
            from.gameObject.name = "From";

            Vector2 size = new Vector2(80f, 50f);
            Text((RectTransform)from.transform, "62", 36f, regular, theme.textSecondary, 0f, Vector2.zero, size);
            TextMeshProUGUI arrow = Text(Child(rect, "Arrow"), "›", 36f, regular, theme.textSecondary, 0f, Vector2.zero, size);
            Text((RectTransform)hitText.transform, "", 36f, heavy, theme.textPrimary, 0f, Vector2.zero, size);
            TextMeshProUGUI delta = Text(Child(rect, "Delta"), "(-4)", 36f, heavy, theme.error, 0f, Vector2.zero, size);

            from.transform.SetSiblingIndex(0);
            arrow.transform.SetSiblingIndex(1);
            hitText.transform.SetSiblingIndex(2);
            delta.transform.SetSiblingIndex(3);
            return new HitRow { group = group, from = from, delta = delta };
        }

        private static GradePresenter BuildGrade(Transform parent, UiShake stageShake)
        {
            RectTransform rect = Child(parent, "Grade");
            Text(rect, "PERFECT", 72f, heavy, theme.success, 4f, new Vector2(0f, -215f), new Vector2(700f, 100f));
            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            var shake = Ensure<UiShake>(rect.gameObject);
            SetFields(shake, ("target", rect));

            var grade = Ensure<GradePresenter>(rect.gameObject);
            SetFields(grade, ("theme", theme), ("label", rect.GetComponent<TextMeshProUGUI>()),
                ("canvasGroup", group), ("labelShake", shake), ("stageShake", stageShake));
            return grade;
        }

        private static GameOverPresenter BuildGameOver(Transform parent)
        {
            RectTransform rect = Child(parent, "GameOver");
            Layout(rect, Center, Center, new Vector2(0f, -285f), new Vector2(700f, 260f));
            var group = Ensure<CanvasGroup>(rect.gameObject);
            group.alpha = 0f;
            group.blocksRaycasts = false;

            RectTransform title = Child(rect, "GameOverText");
            Text(title, "GAME OVER", 72f, heavy, theme.textPrimary, 4f, new Vector2(0f, 70f), new Vector2(700f, 90f));
            RectTransform level = Child(rect, "LevelText");
            TextMeshProUGUI levelText = Text(level, "LEVEL 12", 36f, regular, theme.textSecondary, 6f, Vector2.zero, new Vector2(700f, 50f));

            RectTransform ribbon = Child(rect, "NewBest");
            Layout(ribbon, Center, Center, new Vector2(0f, -65f), new Vector2(260f, 56f));
            Pill(SetImage(ribbon, pill, theme.warn), 56f);
            Text(Child(ribbon, "NewBestText"), "NEW BEST", 28f, heavy, theme.surface, 6f, Vector2.zero, new Vector2(260f, 56f));
            ribbon.gameObject.SetActive(false);

            var presenter = Ensure<GameOverPresenter>(rect.gameObject);
            SetFields(presenter, ("theme", theme), ("canvasGroup", group), ("title", title),
                ("levelText", levelText), ("newBestRibbon", ribbon));
            return presenter;
        }

        private static void StylePrimaryButton(Button button)
        {
            Image image = StyleButtonBase(button, theme.accent);
            var shadow = Ensure<Shadow>(image.gameObject);
            shadow.effectColor = VisualTheme.Hex(0x00000059);
            shadow.effectDistance = new Vector2(0f, -8f);
            shadow.useGraphicAlpha = true;

            Transform ringChild = button.transform.Find("Ring");
            if (ringChild != null) ringChild.gameObject.SetActive(false);
            StyleButtonLabel(button, theme.primaryButtonText);
        }

        private static void StyleSecondaryButton(Button button)
        {
            Image image = StyleButtonBase(button, theme.raised);
            Shadow shadow = image.GetComponent<Shadow>();
            if (shadow != null) Object.DestroyImmediate(shadow, true);

            RectTransform ringRect = Child(button.transform, "Ring");
            ringRect.gameObject.SetActive(true);
            Stretch(ringRect);
            Pill(SetImage(ringRect, ring, VisualTheme.WithAlpha(theme.error, 0.7f)), 140f);
            ringRect.SetAsFirstSibling();
            StyleButtonLabel(button, theme.secondaryButtonText);
        }

        private static Image StyleButtonBase(Button button, Color fill)
        {
            var rect = (RectTransform)button.transform;
            Layout(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, ButtonBottomY), new Vector2(520f, 140f));

            var image = button.GetComponent<Image>();
            image.sprite = pill;
            image.type = Image.Type.Sliced;
            image.color = fill;
            Pill(image, 140f);

            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = VisualTheme.Hex(0xD0D0D0FF);
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var press = Ensure<ButtonPressFeedback>(button.gameObject);
            SetFields(press, ("selectable", button));
            return image;
        }

        private static void StyleButtonLabel(Button button, Color color)
        {
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) return;
            var rect = label.rectTransform;
            Stretch(rect);
            ApplyText(label, label.text, 48f, heavy, color, 6f);
            label.fontStyle = FontStyles.UpperCase;
        }

        private static void ApplyScorePanel()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(ScorePanelPath);
            var panel = root.GetComponent<ScorePanel>();
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.matchWidthOrHeight = 0f;

            RectTransform vignetteRect = Child(root.transform, "Vignette");
            Stretch(vignetteRect);
            SetImage(vignetteRect, vignette, VisualTheme.WithAlpha(Color.white, theme.vignetteStrength));
            vignetteRect.SetAsFirstSibling();

            Transform hpIcon = panel.hpPercentageText.transform.parent == null ? null : FindIcon(panel.hpPercentageText);
            Transform levelIcon = FindIcon(panel.levelText);
            Transform bestIcon = FindIcon(panel.maxLevelText);
            RectTransform hpChip = (RectTransform)hpIcon.parent;
            RectTransform levelChip = (RectTransform)levelIcon.parent;
            RectTransform bestChip = (RectTransform)bestIcon.parent;

            var status = (RectTransform)hpChip.parent;
            Layout(status, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, 72f));
            status.pivot = new Vector2(0.5f, 1f);
            var layout = status.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 24f;
            layout.padding = new RectOffset();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image hpBorder = StyleChip(hpChip, hpIcon, panel.hpPercentageText, 300f, theme.hpIcon);
            StyleChip(levelChip, levelIcon, panel.levelText, 180f, theme.levelIcon);
            StyleChip(bestChip, bestIcon, panel.maxLevelText, 180f, theme.bestIcon);
            // The original prefab wires levelText under the trophy and maxLevelText under the stairs,
            // so assign icons by the field's role rather than trusting the existing sprites.
            hpIcon.GetComponent<Image>().sprite = ProceduralSprites.Load(ProceduralSprites.WhiteCopy(HpIconPath));
            levelIcon.GetComponent<Image>().sprite = ProceduralSprites.Load(ProceduralSprites.WhiteCopy(LevelIconPath));
            bestIcon.GetComponent<Image>().sprite = ProceduralSprites.Load(ProceduralSprites.WhiteCopy(BestIconPath));
            hpChip.SetSiblingIndex(0);
            levelChip.SetSiblingIndex(1);
            bestChip.SetSiblingIndex(2);
            var bestGroup = Ensure<CanvasGroup>(bestChip.gameObject);
            bestGroup.alpha = theme.bestChipAlpha;
            bestGroup.blocksRaycasts = false;

            RectTransform barFill = BuildHpBar(hpChip);
            TextMeshProUGUI[] deltas = BuildDeltaLabels(hpChip);

            var health = Ensure<HealthDisplay>(hpChip.gameObject);
            SetFields(health, ("theme", theme), ("hpText", panel.hpPercentageText), ("barFill", barFill),
                ("chipBorder", hpBorder), ("deltaOrigin", new Vector2(90f, 0f)));
            SetArray(health, "deltaLabels", deltas);
            SetFields(panel, ("healthDisplay", health), ("theme", theme), ("trophyIcon", (RectTransform)bestIcon));

            PrefabUtility.SaveAsPrefabAsset(root, ScorePanelPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static Transform FindIcon(TextMeshProUGUI text)
        {
            // The icon is the text's Image sibling, or its parent in the original layout.
            Transform parent = text.transform.parent;
            if (parent.GetComponent<Image>() != null && parent.GetComponent<LayoutElement>() == null) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name != "Border" && child.name != "HPBar" && child.GetComponent<Image>() != null) return child;
            }
            throw new System.InvalidOperationException("Could not find the icon for " + text.name);
        }

        private static Image StyleChip(RectTransform chip, Transform icon, TextMeshProUGUI text, float width, Color iconColor)
        {
            Image background = Ensure<Image>(chip.gameObject);
            background.sprite = pill;
            background.type = Image.Type.Sliced;
            background.color = VisualTheme.WithAlpha(theme.surface, 0.85f);
            background.raycastTarget = false;
            Pill(background, 72f);

            var element = Ensure<LayoutElement>(chip.gameObject);
            element.preferredWidth = width;
            element.preferredHeight = 72f;

            RectTransform border = Child(chip, "Border");
            Stretch(border);
            Pill(SetImage(border, ring, theme.border), 72f);
            border.SetAsFirstSibling();

            var iconRect = (RectTransform)icon;
            Layout(iconRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(40f, 40f));
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(16f, 0f);
            var iconImage = icon.GetComponent<Image>();
            iconImage.color = iconColor;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            text.transform.SetParent(chip, false);
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(0f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = new Vector2(68f, 0f);
            textRect.sizeDelta = new Vector2(width - 84f, 48f);
            ApplyText(text, "<mspace=0.6em>0</mspace>", 32f, heavy, theme.textPrimary, 0f);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            return border.GetComponent<Image>();
        }

        private static RectTransform BuildHpBar(RectTransform chip)
        {
            RectTransform bar = Child(chip, "HPBar");
            Layout(bar, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -8f), new Vector2(300f, 10f));
            bar.pivot = new Vector2(0.5f, 1f);
            bar.anchoredPosition = new Vector2(0f, -8f);
            Pill(SetImage(bar, pill, theme.raised), 10f);

            RectTransform fill = Child(bar, "Fill");
            Stretch(fill);
            Pill(SetImage(fill, pill, theme.hpIcon), 10f);
            return fill;
        }

        private static TextMeshProUGUI[] BuildDeltaLabels(RectTransform chip)
        {
            var labels = new TextMeshProUGUI[3];
            for (int i = 0; i < labels.Length; i++)
            {
                RectTransform rect = Child(chip, "Delta" + i);
                labels[i] = Text(rect, "", 30f, heavy, theme.error, 0f, new Vector2(90f, 0f), new Vector2(120f, 40f));
                var group = Ensure<CanvasGroup>(rect.gameObject);
                group.alpha = 0f;
                group.blocksRaycasts = false;
                rect.SetAsLastSibling();
            }
            return labels;
        }

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        private static RectTransform Child(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return (RectTransform)existing;
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        private static void Layout(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = Center;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Center;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static Image SetImage(RectTransform rect, Sprite sprite, Color color)
        {
            Image image = Ensure<Image>(rect.gameObject);
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        private static Image Pill(Image image, float height)
        {
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = PillTextureSize / height;
            return image;
        }

        private static TextMeshProUGUI Text(RectTransform rect, string value, float size, Material material, Color color,
            float spacing, Vector2 position, Vector2 boxSize)
        {
            Layout(rect, Center, Center, position, boxSize);
            TextMeshProUGUI text = Ensure<TextMeshProUGUI>(rect.gameObject);
            ApplyText(text, value, size, material, color, spacing);
            return text;
        }

        private static void ApplyText(TextMeshProUGUI text, string value, float size, Material material, Color color, float spacing)
        {
            text.font = font;
            text.fontSharedMaterial = material;
            text.text = value;
            text.fontSize = size;
            text.enableAutoSizing = false;
            text.color = color;
            text.characterSpacing = spacing;
            text.fontStyle = FontStyles.Normal;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            text.raycastTarget = false;
        }

        private static TextMeshProUGUI FirstOtherText(Transform parent, TextMeshProUGUI exclude, params string[] skipNames)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (System.Array.IndexOf(skipNames, child.name) >= 0) continue;
                var text = child.GetComponent<TextMeshProUGUI>();
                if (text != null && text != exclude) return text;
            }
            return null;
        }

        private static void Order(params Transform[] children)
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i].SetSiblingIndex(i);
            }
        }

        private static void SetFields(Object target, params (string name, object value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach ((string name, object value) in fields)
            {
                SerializedProperty property = serialized.FindProperty(name);
                if (property == null)
                {
                    Debug.LogError($"{target.GetType().Name} has no serialized field '{name}'.", target);
                    continue;
                }
                switch (value)
                {
                    case Object obj: property.objectReferenceValue = obj; break;
                    case int number: property.intValue = number; break;
                    case Vector2 vector: property.vector2Value = vector; break;
                    default: Debug.LogError($"Unsupported value for '{name}'."); break;
                }
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string name, Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(name);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
