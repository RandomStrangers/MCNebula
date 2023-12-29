﻿/*
    Copyright 2011 MCForge
        
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
using System.ComponentModel;
using System.Windows.Forms;
using MCNebula.Eco;
using MCNebula.Generator;

namespace MCNebula.Gui {

    public partial class PropertyWindow : Form {
        
        void LoadEcoProps() {
            eco_cbEnabled.Checked = Economy.Enabled;
            eco_txtCurrency.Text = Server.Config.Currency;
            Eco_UpdateEnables();
            
            foreach (Item item in Economy.Items) {
                eco_cmbCfg.Items.Add(item.Name);
            }
            eco_cmbCfg.Items.Add("(none)");
            eco_cmbCfg.SelectedIndex = eco_cmbCfg.Items.Count - 1;

            GuiPerms.SetRanks(eco_cmbItemRank);
            eco_colRankPrice.CellTemplate = new NumericalCell();
            eco_dgvRanks.DataError += eco_dgv_DataError;

            eco_colLvlPrice.CellTemplate = new NumericalCell();
            eco_colLvlX.CellTemplate = new NumericalCell();
            eco_colLvlY.CellTemplate = new NumericalCell();
            eco_colLvlZ.CellTemplate = new NumericalCell();
            eco_colLvlTheme.CellTemplate = new ThemeCell();

            foreach (MapGen gen in MapGen.Generators) {
                if (gen.Type == GenType.Advanced) continue;
                eco_colLvlTheme.Items.Add(gen.Theme);
            }
            eco_dgvMaps.DataError += eco_dgv_DataError;
        }
        
        void ApplyEcoProps() {
            Economy.Enabled = eco_cbEnabled.Checked;
            Server.Config.Currency = eco_txtCurrency.Text;
        }
        
        class NumericalCell : DataGridViewTextBoxCell {
            protected override bool SetValue(int rowIndex, object raw) {
                if (raw == null) return true;
                string str = raw.ToString(); int num;
                
                if (!int.TryParse(str, out num) || num < 0) return false;
                return base.SetValue(rowIndex, raw);
            }
        }
        
        class ThemeCell : DataGridViewComboBoxCell {
            protected override object GetFormattedValue(object value, int rowIndex, 
                                                        ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, 
                                                        TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context) {
                MapGen gen = MapGen.Find((string)value);
                return gen != null ? gen.Theme : null;
            }
        }
        
        
        void Eco_UpdateEnables() {
            eco_lblCurrency.Enabled = eco_cbEnabled.Checked;
            eco_txtCurrency.Enabled = eco_cbEnabled.Checked;
            eco_lblCfg.Enabled = eco_cbEnabled.Checked;
            eco_cmbCfg.Enabled = eco_cbEnabled.Checked;
            
            eco_gbItem.Enabled  = eco_cbEnabled.Checked;
            eco_gbLvl.Enabled = eco_cbEnabled.Checked;
            eco_gbRank.Enabled  = eco_cbEnabled.Checked;
        }
        
        void eco_cbEnabled_CheckedChanged(object sender, EventArgs e) {
            Eco_UpdateEnables();
        }
        
        void Eco_cmbCfg_SelectedIndexChanged(object sender, EventArgs e) {
            string text = "(none)";
            if (eco_cmbCfg.SelectedIndex != -1) {
                text = eco_cmbCfg.SelectedItem.ToString();
            }
            
            eco_gbItem.Visible = false;
            eco_gbLvl.Visible  = false;
            eco_gbRank.Visible = false;
            eco_curItem = null;
            
            Item item = Economy.GetItem(text);
            if (text == "(none)" || item == null) return;
            
            if (item == Economy.Levels) {
                eco_gbLvl.Visible = true;
                eco_cbLvl.Checked = Economy.Levels.Enabled;
                Eco_UpdateLevels();
            } else if (item == Economy.Ranks) {
                eco_gbRank.Visible = true;
                eco_cbRank.Checked = Economy.Ranks.Enabled;
                Eco_UpdateRanks();
            } else if (item is SimpleItem) {
                eco_gbItem.Visible = true;
                eco_curItem = (SimpleItem)item;
                eco_cbItem.Checked = item.Enabled;
                Eco_UpdateItem();
            }
        }
        
        SimpleItem eco_curItem;
        void Eco_UpdateItemEnables() {
            eco_lblItemPrice.Enabled = eco_cbItem.Checked;
            eco_numItemPrice.Enabled = eco_cbItem.Checked;
            eco_lblItemRank.Enabled  = eco_cbItem.Checked;
            eco_cmbItemRank.Enabled   = eco_cbItem.Checked;
        }
        
        void Eco_UpdateItem() {
            eco_gbItem.Text = eco_curItem.Name;
            eco_numItemPrice.Value = eco_curItem.Price;
            Eco_UpdateItemEnables();
            
            GuiPerms.SetSelectedRank(eco_cmbItemRank, eco_curItem.PurchaseRank);
        }
        
        void eco_cbItem_CheckedChanged(object sender, EventArgs e) {
            Eco_UpdateItemEnables();
            eco_curItem.Enabled = eco_cbItem.Checked;
        }
        
        void eco_numItemPrice_ValueChanged(object sender, EventArgs e) {
            eco_curItem.Price = (int)eco_numItemPrice.Value;
        }
        
        void eco_cmbItemRank_SelectedIndexChanged(object sender, EventArgs e) {
            const LevelPermission perm = LevelPermission.Guest;
            if (eco_curItem == null) return;

            eco_curItem.PurchaseRank = GuiPerms.GetSelectedRank(eco_cmbItemRank, perm);
        }

        
        void Eco_UpdateRankEnables() {
            eco_dgvRanks.Enabled = eco_cbRank.Enabled;
        }
        
        void Eco_UpdateRanks() {
            eco_dgvRanks.Rows.Clear();
            foreach (Group grp in Group.GroupList)
            {
                RankItem.RankEntry rank = Economy.Ranks.Find(grp.Permission);
                int price = rank == null ? 0 : rank.Price;

                int idx = eco_dgvRanks.Rows.Add(grp.Name, price);
                eco_dgvRanks.Rows[idx].Tag = grp.Permission;
            } 
            
            Eco_UpdateRankEnables();
        }
        
        void eco_cbRank_CheckedChanged(object sender, EventArgs e) {
            Eco_UpdateRankEnables();
            Economy.Ranks.Enabled = eco_cbRank.Checked;
        }
        
        void eco_dgv_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            string col = eco_dgvMaps.Columns[e.ColumnIndex].HeaderText;
            if (e.ColumnIndex > 0) {
                Popup.Warning(col + " must be an integer greater than zero");
            } else {
                Popup.Warning("Error setting contents of column " + col);
            }
        }
        
        void eco_dgvRanks_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex == -1) return;
            DataGridViewRow row = eco_dgvRanks.Rows[e.RowIndex];
            object price        = row.Cells[1].Value;
            
            // On Mono this event is raised during initialising cells too
            // However, first time event is raised, price is not initialised yet
            if (price == null) return;
            LevelPermission perm = (LevelPermission)row.Tag;
            
            RankItem.RankEntry rank = Economy.Ranks.GetOrAdd(perm);
            rank.Price = int.Parse(price.ToString());
            if (rank.Price == 0) Economy.Ranks.Remove(perm);
        }

        
        void Eco_UpdateLevelEnables() {
            eco_dgvMaps.Enabled   = eco_cbLvl.Checked;
            eco_btnLvlAdd.Enabled = eco_cbLvl.Checked;
            eco_btnLvlDel.Enabled = eco_cbLvl.Checked;
        }
        
        void Eco_UpdateLevels() {
            eco_dgvMaps.Rows.Clear();
            foreach (LevelItem.LevelPreset p in Economy.Levels.Presets) 
            {
                eco_dgvMaps.Rows.Add(p.name, p.price, p.x, p.y, p.z, p.type);
            }
            Eco_UpdateLevelEnables();
        }
        
        void eco_lvlEnabled_CheckedChanged(object sender, EventArgs e) {
            Eco_UpdateLevelEnables();
            Economy.Levels.Enabled = eco_cbLvl.Checked;
        }
        
        void eco_dgvMaps_Apply() {
            List<LevelItem.LevelPreset> presets = new List<LevelItem.LevelPreset>();
            foreach (DataGridViewRow row in eco_dgvMaps.Rows) 
            {
                LevelItem.LevelPreset p = new LevelItem.LevelPreset();
                
                p.name  = row.Cells[0].Value.ToString();
                p.price = int.Parse(row.Cells[1].Value.ToString());
                
                p.x = row.Cells[2].Value.ToString();
                p.y = row.Cells[3].Value.ToString();
                p.z = row.Cells[4].Value.ToString();
                
                p.type = row.Cells[5].Value.ToString();
                presets.Add(p);
            }
            Economy.Levels.Presets = presets;
        }
        
        void eco_dgvMaps_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex >= 0) eco_dgvMaps_Apply();
        }
        
        void eco_lvlAdd_Click(object sender, EventArgs e) {
            string name = "preset_" + (eco_dgvMaps.RowCount + 1);
            eco_dgvMaps.Rows.Add(name, 1000, "64", "64", "64", "flat");
            eco_dgvMaps_Apply();
        }
        
        void eco_lvlDelete_Click(object sender, EventArgs e) {
            if (eco_dgvMaps.SelectedRows.Count == 0) {
                Popup.Warning("No available presets to remove");
            } else {
                DataGridViewRow row = eco_dgvMaps.SelectedRows[0];
                eco_dgvMaps.Rows.Remove(row);
                eco_dgvMaps_Apply();
            }
        }
    }
}
