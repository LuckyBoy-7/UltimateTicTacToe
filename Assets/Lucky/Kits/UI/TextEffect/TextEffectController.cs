using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using Lucky.UI._TMP_Text.TextEffect;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Lucky.Kits.UI.TextEffect
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextEffectController : MonoBehaviour
    {
        private TMP_Text tmpText;
        public HyperLink hyperLink;
        private TMP_TextInfo TextInfo => tmpText.textInfo;

        public float simulationSpeed = 20;

        public int showCharNum;

        // 有odin调试的时候可以开
        [ShowInInspector] public string ParsedString { get; set; }

        // 方便调试
        [Multiline, SerializeField] private string rawContent = "";

        public string RawContent
        {
            get => rawContent;
            set
            {
                rawContent = value;
                charPosToEventInfo.Clear();
                parsedRes.Clear();
                ParseString(); // 设置的时候就要parse，不然update执行顺序会滞后，逻辑错误
                AdjustCharactersVisibility();
            }
        }

        private string preRawContent = ""; // 只是方便调试


        /// 对应字符位置拿到单个标签的信息如[speed=11/], [delay=0.5/] (这里把尖括号换成中括号了, 不然识别不出来)
        /// 搭配dialogueManager食用
        public DefaultDict<int, Dictionary<string, string>> charPosToEventInfo = new(() => new());

        private List<ParsedInfo> parsedRes = new();
        private List<TextEffectBase> textEffects = new();
        private Dictionary<TextEffectType, TextEffectBase> textEffectTypeToTextEffect = new();

        void Awake()
        {
            tmpText = GetComponent<TMP_Text>();
            foreach (TextEffectBase textEffect in new List<TextEffectBase>
                     {
                         new EventEffect(),
                         new ShakeEffect(),
                         new FloatEffect(),
                         new JumpEffect(),
                         new JitterEffect(),
                         new NoneEffect()
                     })
            {
                textEffects.Add(textEffect);
                textEffectTypeToTextEffect[textEffect.textEffectType] = textEffect;
                textEffect.tmpText = tmpText;
            }
        }

        private void Start()
        {
            RawContent = rawContent;
            StartCoroutine(StartEffect());
        }

        private void SetText(string content)
        {
            tmpText.text = "";
            tmpText.text = content;
            tmpText.ForceMeshUpdate(); // 不这么做AdjustCharactersVisibility拿不到正确网格了
        }

        private IEnumerator StartEffect()
        {
            while (true)
            {
                if (RawContent != preRawContent) // 方便测试
                    RawContent = rawContent;
                // 刷新
                tmpText.ForceMeshUpdate();
                // 对文本中的每一段应用效果
                foreach (var parsedInfo in parsedRes)
                    textEffectTypeToTextEffect[parsedInfo.textEffectType].TakeEffect(parsedInfo);
                // 调整字符显示个数
                AdjustCharactersVisibility();

                // 应用
                tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

                yield return new WaitForSeconds(1 / simulationSpeed);
            }
        }

        private void AdjustCharactersVisibility()
        {
            // 不知道为什么空格就是有bug
            // 管理字符显隐
            int j = ParsedString.LastIndexOf(' ');
            // 如果要实现那种字符从左到右淡入的效果, 那么可以让i 0.5 0.5的加, 不过后来想了下可能用的比较少, 所以就先删了
            for (int i = 0; i < ParsedString.Length; i++)
            {
                TMP_CharacterInfo charInfo = TextInfo.characterInfo[i]; // 拿到单个字符信息
                int vertexIndex = charInfo.vertexIndex;
                Color32 color = TextInfo.meshInfo[0].colors32[vertexIndex + 0];
                // color.a = i < Mathf.Max(0, showCharNum) || i == ParsedString.Length - 1 ? Convert.ToByte(255) : Convert.ToByte(0);
                if (i < showCharNum)
                {
                    TextInfo.meshInfo[0].colors32[vertexIndex + 0] = color;
                    TextInfo.meshInfo[0].colors32[vertexIndex + 1] = color;
                    TextInfo.meshInfo[0].colors32[vertexIndex + 2] = color;
                    TextInfo.meshInfo[0].colors32[vertexIndex + 3] = color;
                }
                else
                {
                    color.a = i < Mathf.Max(0, showCharNum) || i == j ? Convert.ToByte(255) : Convert.ToByte(0);
                    SetColor(vertexIndex, color);
                }
            }

            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private void SetColor(int idx, Color32 color)
        {
            TextInfo.meshInfo[0].colors32[idx + 0] = color;
            TextInfo.meshInfo[0].colors32[idx + 1] = color;
            TextInfo.meshInfo[0].colors32[idx + 2] = color;
            TextInfo.meshInfo[0].colors32[idx + 3] = color;
        }


        private void ParseString()
        {
            StringBuilder s = new StringBuilder(rawContent);
            List<ParsedInfo> ranges = new(); // 首位位置，字符串长度，额外信息

            foreach (var textEffect in textEffects)
                textEffect.ParseAndCover(s, ranges);
            // 到此为止s里面只剩下占位符了
            // 按初始位置排序
            ranges.Sort((a, b) => a.start - b.start);
            int realIdx = 0; // 去除所有标签只剩纯文本时对应的idx
            int idx = 0; // 最后要赋值给tmp对应的idx, 也就是相比realIdx多了tmp自带的标签(我们只不过是把自定义标签去除("解析")掉了) 
            foreach (ParsedInfo parsedInfo in ranges)
            {
                if (parsedInfo.textEffectType == TextEffectType.Event)
                {
                    charPosToEventInfo[realIdx].Merge(parsedInfo.args); // merge一下
                    continue;
                }

                SetText(rawContent.Substring(parsedInfo.start, parsedInfo.length));
                int realLength = tmpText.GetParsedText().Length;
                // 最后实际上要应用的片段
                parsedRes.Add(new(parsedInfo.textEffectType, realIdx, realLength, parsedInfo.args));

                // 处理完毕, 下一个
                realIdx += realLength;
                for (int i = 0; i < parsedInfo.length; i++)
                    s[idx++] = rawContent[parsedInfo.start + i];
            }

            SetText(s.ToString().Substring(0, idx));
            preRawContent = rawContent;
            ParsedString = tmpText.GetParsedText();
            hyperLink?.ResetLink();
        }
    }
}