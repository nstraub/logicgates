public class TileEntityButtonPush : TileEntityPoweredTrigger
{

    // ####################################################################
    // ####################################################################

    public TileEntityButtonPush(Chunk _chunk) : base(_chunk) { }

    // ####################################################################
    // ####################################################################

    public override TileEntityType GetTileEntityType()
    {
        // really just an arbitrary number
        // I tend to use number above 241
        return BlockButtonPush.TileEntityType;
    }
    // EO GetTileEntityType

    // ####################################################################
    // ####################################################################

    public override PowerItem CreatePowerItem()
    {
        return new PowerPushButton();
    }
    // EO CreatePowerItem

    // ####################################################################
    // ####################################################################

    public TileEntityButtonPush GetCurcuitRoot()
    {
        if (ConnectionManager.Instance.IsServer)
        {
            var pwr = GetPowerItem() as PowerPushButton;
            var te = pwr.TileEntity;
            return te as TileEntityButtonPush;
        }
        else
        {
            var root = this;
            var world = GameManager.Instance.World;
            while (world.GetTileEntity(0, root.GetParent())
                 is TileEntityButtonPush parent) root = parent;
            return root;
        }
    }
    // EO GetCurcuitRoot

    // ####################################################################
    // ####################################################################

}