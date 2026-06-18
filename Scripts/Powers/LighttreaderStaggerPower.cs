using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Afflictions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 踏光兽给玩家挂上的持续性压制。
/// 每个玩家回合至多标记指定数量的手牌，避免一回合滚到失控。
/// </summary>
[CustomID("ECHO_CORE_LIGHTTREADER_STAGGER_POWER")]
public sealed class LighttreaderStaggerPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/monsters/wuwa/lighttreader_beast.webp";

    private sealed class Data
    {
        public int AfflictedThisTurn;
    }

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<LighttreaderStaggeredAffliction>();

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "震慑",
            "你的回合中，每回合会有若干张抽到的手牌被随机附加震慑。被震慑的牌费用增加 1，获得保留；打出后，所有敌人获得 1 点力量。",
            "你的回合中，每回合至多有 {Amount:diff()} 张抽到的手牌被随机附加震慑。被震慑的牌费用增加 1，获得保留；打出后，所有敌人获得 1 点力量。"),
        _ => new PowerLoc(
            "Stagger",
            "On your turn, some drawn cards each turn are randomly afflicted with Staggered. Staggered cards cost 1 more and gain Retain. When played, all enemies gain 1 Strength.",
            "On your turn, up to {Amount:diff()} drawn cards each turn are randomly afflicted with Staggered. Staggered cards cost 1 more and gain Retain. When played, all enemies gain 1 Strength.")
    };

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner || CombatState.CurrentSide != Owner.Side)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (data.AfflictedThisTurn >= Amount)
        {
            return;
        }

        if (Owner.Player == null)
        {
            return;
        }

        var player = Owner.Player;
        var playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
        {
            return;
        }

        List<CardModel> candidates = playerCombatState.Hand.Cards
            .Where(c => c.Affliction == null && ModelDb.Affliction<LighttreaderStaggeredAffliction>().CanAfflict(c))
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        CardModel? chosen = player.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        if (chosen == null)
        {
            return;
        }

        LighttreaderStaggeredAffliction? affliction = await CardCmd.Afflict<LighttreaderStaggeredAffliction>(chosen, 1m);
        if (affliction == null)
        {
            return;
        }

        chosen.EnergyCost.AddThisTurnOrUntilPlayed(1);
        data.AfflictedThisTurn++;
        Flash();
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            GetInternalData<Data>().AfflictedThisTurn = 0;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented && creature == Applier)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        IEnumerable<CardModel> allCards = oldOwner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
        foreach (CardModel card in allCards)
        {
            if (card.Affliction is LighttreaderStaggeredAffliction)
            {
                CardCmd.ClearAffliction(card);
            }
        }

        return Task.CompletedTask;
    }
}
