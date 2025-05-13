using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElectricityButtonsPush.Harmony
{
    public class LogicGateControl : MonoBehaviour
    {
        public WorldBase world;
        public Vector3i position;
        public int clrIdx;
        public BlockButtonPush block { get; set; }
        public BlockValue bv { get; set; }
        
        void Update()
        {
            var bv2 = bv;
            if (Nand(world, clrIdx, position))
            {
                bv2.meta = (byte)(bv.meta | 0b100);
            }
            world.SetBlockRPC(clrIdx, position, bv2);
        }
        private bool Nand(WorldBase world, int clrIdx, Vector3i position)
        {
            var leftPos = new Vector3i(position.x, position.y + 1, position.z);
            var belowPos = new Vector3i(position.x, position.y - 1, position.z);

            var aboveTile = world.GetTileEntity(clrIdx, leftPos) as TileEntityPoweredBlock;
            var belowTile = world.GetTileEntity(clrIdx, belowPos) as TileEntityPoweredBlock;
            if (aboveTile == null) return false;
            if (belowTile == null) return false;

            var abovePi = aboveTile.GetPowerItem();

            var belowPi = belowTile.GetPowerItem();

            return !(abovePi.isPowered && belowPi.isPowered);
        }
    }
}
