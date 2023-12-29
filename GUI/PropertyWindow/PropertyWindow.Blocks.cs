﻿/*
    Copyright 2015 MCGalaxy
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MCNebula.Blocks;
using BlockID = System.UInt16;
using BlockRaw = System.Byte;

namespace MCNebula.Gui {
    public partial class PropertyWindow : Form {
        BlockID curBlock;
        List<BlockID> blockIDMap;
        ItemPermsHelper blockItems = new ItemPermsHelper();
        
        // need to keep a list of changed block perms, because we don't want
        // to modify the server's live permissions if user clicks 'discard'
        BlockPerms placePermsOrig, placePermsCopy;
        List<BlockPerms> placePermsChanged = new List<BlockPerms>();
        BlockProps[] blockPropsChanged = new BlockProps[Block.Props.Length];
        // TODO delete permissions too
        
        void LoadBlocks() {
            blk_list.Items.Clear();
            placePermsChanged.Clear();
            blockIDMap = new List<BlockID>();
            
            for (int b = 0; b < blockPropsChanged.Length; b++) 
            {
                blockPropsChanged[b] = Block.Props[b];
                blockPropsChanged[b].ChangedScope = 0;
                
                BlockID block = (BlockID)b;
                if (!Block.ExistsGlobal(block)) continue;
                
                string name = Block.GetName(Player.Console, block);
                blk_list.Items.Add(name);
                blockIDMap.Add(block);
            }
            
            blockItems.GetCurPerms = BlockGetOrAddPermsChanged;
            if (blk_list.SelectedIndex == -1) {
                blk_list.SelectedIndex = 0;
            }
        }

        void SaveBlocks() {
            if (placePermsChanged.Count > 0)
                SaveBlockPermissions();            
            if (AnyBlockPropsChanged())
                SaveBlockProps();
            
            LoadBlocks();
        }
        
        void SaveBlockPermissions() {
            foreach (BlockPerms changed in placePermsChanged) 
            {
                BlockPerms pOrig = BlockPerms.GetPlace(changed.ID);
                changed.CopyPermissionsTo(pOrig);
                
                BlockPerms dOrig = BlockPerms.GetDelete(changed.ID);
                changed.CopyPermissionsTo(dOrig);
            }
            
            BlockPerms.Save();
            BlockPerms.ApplyChanges();
            BlockPerms.ResendAllBlockPermissions();
        }
        
        bool AnyBlockPropsChanged() {
            for (int b = 0; b < blockPropsChanged.Length; b++) 
            {
                if (blockPropsChanged[b].ChangedScope != 0) return true;
            }
            return false;
        }
        
        void SaveBlockProps() {
            for (int b = 0; b < blockPropsChanged.Length; b++) 
            {
                if (blockPropsChanged[b].ChangedScope == 0) continue;
                Block.Props[b] = blockPropsChanged[b];
            }
            
            BlockProps.Save("default", Block.Props, 1); 
            Block.SetBlocks();
        }
        
        
        void blk_list_SelectedIndexChanged(object sender, EventArgs e) {
            curBlock = blockIDMap[blk_list.SelectedIndex];
            placePermsOrig = BlockPerms.GetPlace(curBlock);
            placePermsCopy = placePermsChanged.Find(p => p.ID == curBlock);
            
            BlockInitSpecificArrays();
            blockItems.SupressEvents = true;
            
            BlockProps props = blockPropsChanged[curBlock];
            blk_cbMsgBlock.Checked = props.IsMessageBlock;
            blk_cbPortal.Checked = props.IsPortal;
            blk_cbDeath.Checked = props.KillerBlock;
            blk_txtDeath.Text = props.DeathMessage;
            blk_txtDeath.Enabled = blk_cbDeath.Checked;
            
            blk_cbDoor.Checked = props.IsDoor;
            blk_cbTdoor.Checked = props.IsTDoor;
            blk_cbRails.Checked = props.IsRails;
            blk_cbLava.Checked = props.LavaKills;
            blk_cbWater.Checked = props.WaterKills;
            
            BlockPerms perms = placePermsCopy != null ? placePermsCopy : placePermsOrig;
            blockItems.Update(perms);
        }
        
        void BlockInitSpecificArrays() {
            if (blockItems.MinBox != null) return;
            blockItems.MinBox = blk_cmbMin;
            blockItems.AllowBoxes = new ComboBox[] { blk_cmbAlw1, blk_cmbAlw2, blk_cmbAlw3 };
            blockItems.DisallowBoxes = new ComboBox[] { blk_cmbDis1, blk_cmbDis2, blk_cmbDis3 };
            blockItems.FillInitial();
        }
        
        ItemPerms BlockGetOrAddPermsChanged() {
            if (placePermsCopy != null) return placePermsCopy;
            placePermsCopy = placePermsOrig.Copy();
            placePermsChanged.Add(placePermsCopy);
            return placePermsCopy;
        }
        
        
        void blk_cmbMin_SelectedIndexChanged(object sender, EventArgs e) {
            blockItems.OnMinRankChanged((ComboBox)sender);
        }
        
        void blk_cmbSpecific_SelectedIndexChanged(object sender, EventArgs e) {
            blockItems.OnSpecificChanged((ComboBox)sender);
        }

        void blk_btnHelp_Click(object sender, EventArgs e) {
            GetHelp(blk_list.SelectedItem.ToString());
        }
        
        
        void blk_cbMsgBlock_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].IsMessageBlock = blk_cbMsgBlock.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_cbPortal_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].IsPortal = blk_cbPortal.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_cbDeath_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].KillerBlock = blk_cbDeath.Checked;
            blk_txtDeath.Enabled = blk_cbDeath.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_txtDeath_TextChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].DeathMessage = blk_txtDeath.Text;
            MarkBlockPropsChanged();
        }
        
        void blk_cbDoor_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].IsDoor = blk_cbDoor.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_cbTdoor_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].IsTDoor = blk_cbTdoor.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_cbRails_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].IsRails = blk_cbRails.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_cbLava_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].LavaKills = blk_cbLava.Checked;
            MarkBlockPropsChanged();
        }
        
        void blk_cbWater_CheckedChanged(object sender, EventArgs e) {
            blockPropsChanged[curBlock].WaterKills = blk_cbWater.Checked;
            MarkBlockPropsChanged();
        }
        
        void MarkBlockPropsChanged() {
            // don't mark props as changed when supressing events
            int changed = blockItems.SupressEvents ? 0 : BlockProps.SCOPE_GLOBAL;
            blockPropsChanged[curBlock].ChangedScope = (byte)changed;
        }
    }
}
