using JesusTheChrist.Presentation.Views;

namespace JesusTheChrist.Presentation.Tests.Views;

public class ScrollAnchorTests
{
    [Fact]
    public void Resolve_CardFillsViewport_NothingPeeks_ReturnsEnd()
    {
        Assert.Equal(ScrollAnchorPosition.End, ScrollAnchor.Resolve(cardIndex: 2, firstVisible: 2, lastVisible: 2));
    }

    [Fact]
    public void Resolve_CardFillsViewport_NextPeeks_ReturnsEnd()
    {
        Assert.Equal(ScrollAnchorPosition.End, ScrollAnchor.Resolve(cardIndex: 2, firstVisible: 2, lastVisible: 3));
    }

    [Fact]
    public void Resolve_ShortCard_MultipleItemsBelow_ReturnsMakeVisible()
    {
        Assert.Equal(ScrollAnchorPosition.MakeVisible, ScrollAnchor.Resolve(cardIndex: 2, firstVisible: 2, lastVisible: 4));
    }

    [Fact]
    public void Resolve_NoScrollYet_ReturnsMakeVisible()
    {
        Assert.Equal(ScrollAnchorPosition.MakeVisible, ScrollAnchor.Resolve(cardIndex: 2, firstVisible: null, lastVisible: null));
    }

    [Fact]
    public void Resolve_CardNotFirstVisible_ReturnsMakeVisible()
    {
        Assert.Equal(ScrollAnchorPosition.MakeVisible, ScrollAnchor.Resolve(cardIndex: 2, firstVisible: 1, lastVisible: 3));
    }
}
