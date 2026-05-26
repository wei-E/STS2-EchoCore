using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// Echo Core 主动技卡公共基类。主动技卡只由声骸按钮生成，不进入常规奖励池。
/// </summary>
[Pool(typeof(TokenCardPool))]
public abstract class EchoCoreCard(
    int baseCost,
    CardType type,
    TargetType target,
    CardRarity rarity = CardRarity.Token
) : CustomCardModel(baseCost, type, rarity, target, showInCardLibrary: false)
{
    public override string? CustomPortraitPath => "res://echo-core/ui/echoes/icons/default_echo_icon.png";
}
