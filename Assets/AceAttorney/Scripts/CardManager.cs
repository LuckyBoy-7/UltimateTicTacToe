using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AceAttorney.Scripts.Card.Logic;
using AceAttorney.Scripts.Card.Logic.Cards;
using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Collections;
using Lucky.Kits.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Ease = DG.Tweening.Ease;

[Serializable]
public class CharacterCardConfig
{
    public CharacterCard prefab;
    public Color color;
    public int cardDeckCapacity;

    // 法官可以直接配置, 律师和检察官的要覆盖
    public Vector2Int position;
    public CardDeck cardDeck;
}

[Serializable]
public class LeftRightPlayerConfig
{
    public Vector2Int position;
    public CardDeck cardDeck;
}

public class CardManager : Engine
{
    public static CardManager Instance { get; set; }

    [Title("Configs")] public CharacterCardConfig judgeConfig;
    public CharacterCardConfig lawyerConfig;
    public CharacterCardConfig procuratorConfig;
    public LeftRightPlayerConfig leftPlayerConfig;
    public LeftRightPlayerConfig rightPlayerConfig;
    public Color trueColor;
    public Color falseColor;
    public Color validColor;
    public Color invalidColor;
    public int CardGap = 110;
    public int CardSize = 110;
    public float swapCardDuration = 0.2f;
    public bool duringUnsafeAnim;

    // 所有牌的载体
    [FormerlySerializedAs("canvas")] public Transform worldCanvas;
    private Vector3 CardPivotPosition => worldCanvas.position;

    [Title("Cards")] public UsableCard lawyerRetainedCard;
    public UsableCard procuratorRetainedCard;

    public List<TestimonyCard> testimonyCardPrefabs;
    public List<UsableCard> playerCardPrefabs;
    public List<UsableCard> debugProcuratorCardPrefabs;
    public List<UsableCard> debugLawyerCardPrefabs;
    public bool debug;

    public JudgeCard judge;
    public PlayerCharacterCard procurator;
    public PlayerCharacterCard lawyer;

    public int round;

    [ShowInInspector] private GridManager<Card> gridManager;

    public CharacterCard currentRoundPlayer;
    public CharacterCard currentInControlCharacter;
    public UsableCard currentSelectedCard;

    public List<TestimonyCard> onBoardTestimonyCards = new();
    public CharacterCard counterCharacter;
    public bool InCounterState => counterCharacter != null;
    public bool InReverseState { get; set; } = false;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        gridManager = new GridManager<Card>(CardGap, CardGap, true, () => CardPivotPosition); 
    }

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return ChooseSide();
        yield return GameMainFlow();
    }

    private IEnumerator ChooseSide()
    {
        InstantiateCharacterCard(out judge, judgeConfig);

        // 选边
        bool flip = RandomUtils.Random01() == 1;
        // 默认律师在左边
        if (!flip)
        {
            OverrideCharacterConfigByLeftRightPlayerConfig(lawyerConfig, leftPlayerConfig);
            OverrideCharacterConfigByLeftRightPlayerConfig(procuratorConfig, rightPlayerConfig);
        }
        else
        {
            OverrideCharacterConfigByLeftRightPlayerConfig(lawyerConfig, rightPlayerConfig);
            OverrideCharacterConfigByLeftRightPlayerConfig(procuratorConfig, leftPlayerConfig);
        }

        InstantiateCharacterCard(out lawyer, lawyerConfig);
        InstantiateCharacterCard(out procurator, procuratorConfig);
        if (flip)
            lawyer.TurnToLeft();
        else
            procurator.TurnToLeft();
        yield break;
    }

    private void OverrideCharacterConfigByLeftRightPlayerConfig(CharacterCardConfig characterCardConfig, LeftRightPlayerConfig leftRightPlayerConfig)
    {
        characterCardConfig.cardDeck = leftRightPlayerConfig.cardDeck;
        characterCardConfig.position = leftRightPlayerConfig.position;
    }

    private IEnumerator GameMainFlow()
    {
        FillCardDeckData();
        yield return judge.DealCardsUntilCardDeckIsFull();
        yield return lawyer.DealCardsUntilCardDeckIsFull();
        yield return procurator.DealCardsUntilCardDeckIsFull();

        while (true)
        {
            yield return procurator.StartRound();
            yield return lawyer.StartRound();
        }
    }

    /// <summary>
    /// 给对应角色的牌堆塞对应种类的牌, 到时候发的牌从里面抽
    /// </summary>
    private void FillCardDeckData()
    {
        judge.cardDeck.FillCardData(testimonyCardPrefabs.ToList<UsableCard>());
        procurator.cardDeck.FillCardData(debug ? debugProcuratorCardPrefabs : playerCardPrefabs);
        lawyer.cardDeck.FillCardData(debug ? debugLawyerCardPrefabs : playerCardPrefabs);
    }


    private void InstantiateCharacterCard<T>(out T ins, CharacterCardConfig config) where T : CharacterCard
    {
        ins = Instantiate(config.prefab, worldCanvas) as T;
        ins.InitWithConfig(config);
    }


    public Card GetUsedCardAt(Vector3 worldPosition) => gridManager[worldPosition];
    public Vector2Int GetGridPos(Vector3 worldPosition) => gridManager.ToGridPos(worldPosition);

    public Vector2 SnapPosToGridCenter(Vector2 worldPosition) => gridManager.ToCenterPos(worldPosition);


    public void PlaceCardAt(Card card, Vector2Int gridPos)
    {
        gridManager.SetItem(card, gridPos);
        card.transform.position = gridManager.ToWorldPos(gridPos);
    }

    public void UpdateCharacterCardScore()
    {
        lawyer.UpdateScore();
        procurator.UpdateScore();
    }

    public void UpdateCards()
    {
        foreach (TestimonyCard card in onBoardTestimonyCards)
        {
            card.UpdateCard();
        }

        UpdateCharacterCardScore();
    }

    public void SwapTwoTestimonyCards(TestimonyCard from, TestimonyCard to)
    {
        Vector2 fromPos = from.transform.position;
        Vector2 toPos = to.transform.position;
        duringUnsafeAnim = true;
        from.transform.DOMove(toPos, swapCardDuration).SetEase(Ease.OutSine);
        to.transform.DOMove(fromPos, swapCardDuration).SetEase(Ease.OutSine).onComplete += () =>
        {
            (from.depth, to.depth) = (to.depth, from.depth);
            gridManager[from.GridPos] = from;
            gridManager[to.GridPos] = to;
            duringUnsafeAnim = false;
            UpdateCards();
        };
    }
}