using UnityEngine;
using static PowerItem;
using System.Collections.Generic;
using System;
using ElectricityButtonsPush.Harmony;
using ElectricityButtonsPush.Harmony.Gates;

public class BlockButtonPush : BlockPowered
{

    // ####################################################################
    // ####################################################################
    private static readonly bool debug = true;
    public static PowerItemTypes PowerItemType = (PowerItemTypes)243;
    public static TileEntityType TileEntityType = (TileEntityType)243;
    private Gate _gate;

    private Gate Gate
    {
        get { return _gate ?? (_gate = new NandGate()); }
    }

    // ####################################################################
    // Basic commands available when pressing `E` for a few seconds
    // If only one command is enabled, pressing `E` will use that directly
    // See `GetBlockActivationCommands` where options get enabled/disabled
    // ####################################################################
    private void Log(string msg)
    {
        if (debug) Debug.Log(msg);
    }

    private new readonly BlockActivationCommand[] cmds = new BlockActivationCommand[2]
    {
        new BlockActivationCommand("light", "electric_switch", true),
        new BlockActivationCommand("take", "hand", false)
    };

    // ####################################################################
    // Basic Constructor (configure base settings)
    // ####################################################################

    public BlockButtonPush() => HasTileEntity = true;

    // ####################################################################
    // Method is only called on client side where graphics are drawn
    // To setup the model to the stored state after it is created
    // ####################################################################

    public override void OnBlockEntityTransformAfterActivated(
        WorldBase world, Vector3i position,
        int clrIdx, BlockValue bv,
        BlockEntityData ebcd)
    {
        var go = ebcd.transform.gameObject;
        if (go != null)
        {
            var control = go.GetComponent<LogicGateControl>() ?? go.AddComponent<LogicGateControl>();
            if (control != null)
            {
                control.world = world;
                control.clrIdx = clrIdx;
                control.position = position;
                control.block = this;
                control.bv = bv;
            }
        }
        // Base code will set `TileEntityPowered.BlockTransform` from `ebcd`
        // It will also start a coroutine to draw connected wires asynchronous
        base.OnBlockEntityTransformAfterActivated(world, position, clrIdx, bv, ebcd);
        if (!bv.ischild && world.GetTileEntity(clrIdx, position) is TileEntityPoweredTrigger te) UpdateVisualState(world, clrIdx, position, bv, te.IsTriggered);
    }

    // ####################################################################
    // Method to create a tile entity when needed
    // E.g. on clients after block is added via server
    // It seems none of our base classes is really calling it
    // See our `OnBlockAdded` to see where we actually use it
    // Note: could also just have our own method, but play along
    // ####################################################################

    public override TileEntityPowered CreateTileEntity(Chunk chunk)
    {
        return new TileEntityButtonPush(chunk)
        {
            PowerItemType = PowerItemType,
            TriggerType = PowerTrigger.TriggerTypes.Motion
        };
    }
    // EO CreateTileEntity

    // ####################################################################
    // Method to return a string for anyone looking at it
    // This will be displayed as the "overlay" text for the block
    // ####################################################################

    public override string GetActivationText(
        WorldBase world, BlockValue bv,
        int clrIdx, Vector3i position,
        EntityAlive watcher)
    {
        // Check if we are not the master block position
        // TileEntity is only found there for multi-dims
        // Return the localized label to show to focusing entity
        return Localization.Get("ocbBlockPushPowerButton");
    }
    // EO GetActivationText


    // ####################################################################
    // Invoked when the `BlockValue` has been changed
    // Main method to transfer triggers to the server
    // We use 3rd bit of meta to indicate a client toggle
    // ####################################################################

    public override void OnBlockValueChanged(
        WorldBase world, Chunk chunk,
        int clrIdx, Vector3i position,
        BlockValue old_bv, BlockValue new_bv)
    {
        var isTriggered = false;
        //    Log("clrIdx: " + clrIdx);
        //    Log("postition x: " + position.x + ", y: " + position.y + ", z: " + position.z);
        // Process the toggle event on the server (dispatch to circuit root 
        if (ConnectionManager.Instance.IsServer && world.GetTileEntity(clrIdx, position) is TileEntityPoweredTrigger te)
        {
            te.IsTriggered = isTriggered = ShouldTrigger(world, clrIdx, position);
        }

        // A change may also indicate that the whole block was replaced
        base.OnBlockValueChanged(world, chunk, clrIdx, position, old_bv, new_bv);

        // Update visual state when server syncs to clients
        UpdateVisualState(world, clrIdx, position, new_bv, isTriggered);
    }
    // EO OnBlockValueChanged

    // ####################################################################
    // Method is called once user has activated an available command
    // See `GetBlockActivationCommands` for the commands available
    // ####################################################################

    public override bool OnBlockActivated(
        string cmd,
        WorldBase world,
        int clrIdx,
        Vector3i position,
        BlockValue bv,
        EntityPlayerLocal player)
    {
        // Check for master block
        if (cmd == "light")
        {
            // toggle local trigger flag. Server will properly  
            // dispatch event to the circuit root when received
            bv.meta ^= 0b100;
            // Broadcast changes to server
            world.SetBlockRPC(clrIdx, position, bv);
        }
        return true;
    }
    // EO OnBlockActivated

    // ####################################################################
    // Main entry method when TileEntity wants us to update
    // E.g. called when wiring on the TileEntity changes
    // So in fact this sets if an item is powered or not
    // ####################################################################

    public override bool ActivateBlock(
        WorldBase world, int clrIdx,
        Vector3i position, BlockValue bv,
        bool isActive, bool isPowered)
    {
        base.ActivateBlock(world, clrIdx, position, bv, isActive, isPowered);
        bv.meta = (byte)(bv.meta & ~(0b11));
        if (isPowered) bv.meta |= 0b1;
        if (ShouldTrigger(world, clrIdx, position)) bv.meta |= 0b110;
        world.SetBlockRPC(clrIdx, position, bv);
        return true;
    }

    // EO ActivateBlock
    private bool ShouldTrigger(WorldBase world, int clrIdx, Vector3i position)
    {
        var leftPos = new Vector3i(position.x, position.y + 1, position.z);
        var belowPos = new Vector3i(position.x, position.y - 1, position.z);
        

        var aboveTile = world.GetTileEntity(clrIdx, leftPos) as TileEntityPoweredBlock;
        var belowTile = world.GetTileEntity(clrIdx, belowPos) as TileEntityPoweredBlock;

        if (aboveTile == null || belowTile == null) return false;
        var belowPowered = belowTile?.GetPowerItem()?.isPowered ?? false;
        var abovePowered = aboveTile?.GetPowerItem()?.isPowered ?? false;
        

        return Gates.Evaluate(blockName, belowPowered, abovePowered);
    }
    // ####################################################################
    // ####################################################################

    private Transform GetBlockEntityTransform(WorldBase world, int clrIdx, Vector3i position)
    {
        // Check if cluster and chunk is actually loaded at the moment
        if (!(world.ChunkClusters[clrIdx] is ChunkCluster cluster)) return null;
        if (!(cluster.GetChunkFromWorldPos(position) is Chunk chunk)) return null;
        // Try to fetch the BlockEntityData's transform
        return chunk.GetBlockEntity(position)?.transform;
    }
    // EO GetBlockEntityTransform


    // ####################################################################
    // Update local visual state according to `BlockValue`
    // ####################################################################

    private void UpdateVisualState(
        WorldBase world, int clrIdx,
        Vector3i position, BlockValue bv, bool toggled)
    {
        if (GameManager.IsDedicatedServer) return;
        // Extract two flags from meta data
        bool powered = (bv.meta & 0b1) == 0b1;
        
        // Get the associated TileEntity with block
        var te = world.GetTileEntity(clrIdx, position) as TileEntityPoweredTrigger;
        

        // Either use cached transform from TileEntityPowered ...
        Transform transform = te?.BlockTransform ??
            // ... or fetch the data directly from the cluster
            GetBlockEntityTransform(world, clrIdx, position);

        // Ensure model is available
        if (transform != null)
        {
            // Set button color according to powered and toggle state
            Color color = powered ? toggled ? Color.green : Color.red : Color.yellow;
            // Process all renderers in this model to apply new visual settings
            foreach (var child in transform.GetComponentsInChildren<Renderer>())
            {
                // We only apply effect to tagger children
                if (child.tag != "T_Deco") continue;
                // Check and operation copied from `BlockSwitch`
                if (child.material != child.sharedMaterial)
                    child.material = new Material(child.sharedMaterial);
                // Update material properties for colors
                child.material.SetColor("_Emission", color);
                // Enable shader keywords to switched compiled variant
                if (powered) child.material.EnableKeyword("EMISSION_ON");
                else child.material.DisableKeyword("EMISSION_ON");
                // Copy back (as seen in vanilla code)
                child.sharedMaterial = child.material;
            }
        }
    }
    // EO UpdateVisualState

    // ####################################################################
    // Method is extecuted to determine available options to user
    // This is only called on the client side (where world is rendered)
    // Note that this will be executed for all child blocks for multi-dims
    // See `OnBlockActivated` where any action will be redirected to master
    // ####################################################################

    public override BlockActivationCommand[] GetBlockActivationCommands(
        WorldBase world, BlockValue bv,
        int clrIdx, Vector3i position,
        EntityAlive watcher)
    {
        // Seems to be OK to assume that watcher is the local player
        var player = world.GetGameManager().GetPersistentLocalPlayer();
        // Allow toggle and options commands if player is allowed to place a block here
        cmds[0].enabled = world.CanPlaceBlockAt(position, player);
        // Allow to pick up the block if block is within our land claim
        cmds[1].enabled = world.IsMyLandProtectedBlock(position, player);
        // Return command options
        return cmds;
    }
    // EO GetBlockActivationCommands

    // ####################################################################
    // ####################################################################

}