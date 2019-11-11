﻿using DevExpress.SpreadsheetSource.Implementation;
using DevExpress.Utils;
using DevExpress.XtraDashboardLayout;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CSDLPT {
    public partial class ClassForm : DevExpress.XtraEditors.XtraForm {

        private String tenPhanManh = string.Empty;
        private string tenServer = string.Empty;
        private String maKhoa = "CNTT";
        private int vitri;
        private int vitriSV;
        private DataGridViewCell currentCell;
        private int dataGridViewMode = 0;

        public ClassForm() {
            InitializeComponent();
            //this.ContextMenuStrip = contextMenuStrip1;
            gridView1.OptionsDetail.EnableMasterViewMode = false;
        }

        private void ClassForm_Load(object sender, EventArgs e) {
            tenPhanManh = ((DataRowView)Program.bds_dspm.Current)["TENPM"].ToString();
            tenServer = ((DataRowView)Program.bds_dspm.Current)["TENSERVER"].ToString();
            dS_SERVER1.EnforceConstraints = false;
            try {
                this.taSV.Connection.ConnectionString = Program.connstr;
                this.taLop.Connection.ConnectionString = Program.connstr;
                this.taKhoa.Connection.ConnectionString = Program.connstr;
                this.taDiem.Connection.ConnectionString = Program.connstr;
                this.taHocPhi.Connection.ConnectionString = Program.connstr;
                this.taAllClass.Connection.ConnectionString = Program.connstr;

                // TODO: This line of code loads data into the 'dS_SERVER1.KHOA' table. You can move, or remove it, as needed.
                this.taKhoa.Fill(this.dS_SERVER1.KHOA);
                // TODO: This line of code loads data into the 'dS_SERVER1.LOP' table. You can move, or remove it, as needed.
                this.taLop.Fill(this.dS_SERVER1.LOP);
                // TODO: This line of code loads data into the 'dS_SERVER1.SINHVIEN' table. You can move, or remove it, as needed.
                this.taSV.Fill(this.dS_SERVER1.SINHVIEN);
                // TODO: This line of code loads data into the 'dS_SERVER1.DIEM' table. You can move, or remove it, as needed.
                this.taDiem.Fill(this.dS_SERVER1.DIEM);
                // TODO: This line of code loads data into the 'dS_SERVER1.HOCPHI' table. You can move, or remove it, as needed.
                this.taHocPhi.Fill(this.dS_SERVER1.HOCPHI);
                // TODO: This line of code loads data into the 'dS_SERVER1.AllClass' table. You can move, or remove it, as needed.
                this.taAllClass.AllClass(this.dS_SERVER1.AllClass);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            //Xóa mảnh 3
            BindingSource bds_dspm_currentForm = new BindingSource();
            bds_dspm_currentForm.DataSource = Program.bds_dspm.DataSource;
            if (bds_dspm_currentForm.Count.Equals(3))
                bds_dspm_currentForm.RemoveAt(bds_dspm_currentForm.Count - 1);

            this.cmbKhoaInUse.DataSource = bds_dspm_currentForm.DataSource;
            this.cmbKhoaInUse.SelectedIndex = Program.mChinhanh;
            this.cmbKhoaInUse.DisplayMember = "TENPM";
            this.cmbKhoaInUse.ValueMember = "TENSERVER";
            this.cmbKhoaInUse.Enabled = true;

            //Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            //View control
            gcLop.Enabled = true;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = true;
        }

        private void barButtonAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            //Vị trí của item chọn
            vitri = bdsLop.Position;
            groupBox1.Enabled = true;
            gcLop.Enabled = false;
            bdsLop.AddNew();
            txbMaKhoa.Text = maKhoa;
            txbMaLop.ReadOnly = false;

            //Bar button control
            bbtnAdd.Enabled = false;
            bbtnEdit.Enabled = false;
            bbtnRemove.Enabled = false;
            bbtnRecovery.Enabled = true;
            bbtnWrite.Enabled = true;

            cmbKhoaInUse.Enabled = false;
        }

        private void bbtnWrite_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {

            //Validate
            txbMaLop.Text = Regex.Replace(txbMaLop.Text.Trim(), @"\s+", string.Empty);
            txbTenLop.Text = Regex.Replace(txbTenLop.Text.Trim(), @"\s+", " ");
            if (txbMaLop.Text == string.Empty) {
                MessageBox.Show("Mã lớp không được để trống!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbMaLop.Focus();
                return;
            }
            if (txbMaLop.Text.Length > 8) {
                MessageBox.Show("Mã lớp phải nhỏ hơn hoặc bằng 8 ký tự!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbMaLop.Focus();
                return;
            }
            if (txbTenLop.Text == string.Empty) {
                MessageBox.Show("Tên lớp không được để trống!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbTenLop.Focus();
                return;
            }
            if (txbTenLop.Text.Length > 40) {
                MessageBox.Show("Tên lớp phải nhỏ hơn hoặc bằng 40 ký tự!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbTenLop.Focus();
                return;
            }

            //Update database
            try {
                bdsLop.EndEdit();
                bdsLop.ResetCurrentItem();
                if (dS_SERVER1.HasChanges())
                    taLop.Update(dS_SERVER1.LOP);

            } catch (SqlException err) {
                Console.WriteLine(err.Message);
                if (err.Message.Contains("PRIMARY")) {
                    MessageBox.Show("Mã lớp bị trùng!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txbMaLop.Focus();
                    return;
                } else if (err.Message.Contains("UNIQUE")) {
                    MessageBox.Show("Tên lớp bị trùng!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txbTenLop.Focus();
                    return;
                } else {
                    MessageBox.Show("Lỗi tạo lớp, vui lòng xem lại!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRecovery.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            gcLop.Enabled = true;
            groupBox1.Enabled = false;

            cmbKhoaInUse.Enabled = true;
        }

        private void bbtnEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            bbtnAdd.Enabled = false;
            bbtnEdit.Enabled = false;
            bbtnRemove.Enabled = false;
            bbtnWrite.Enabled = true;
            bbtnRecovery.Enabled = true;
            //bbtnRefresh.Enable = true;
            groupBox1.Enabled = true;
            gcLop.Enabled = false;
            vitri = bdsLop.Position;

            cmbKhoaInUse.Enabled = false;
            return;
        }

        private void bbtnRemove_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            if (bdsSV.Count > 0) {
                MessageBox.Show("Lớp có sinh viên không được xóa!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Bạn muốn xóa lớp này?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes) {
                try {
                    bdsLop.RemoveCurrent();
                    taLop.Update(dS_SERVER1.LOP);
                    return;
                } catch (Exception err) {
                    Console.WriteLine(err.Message);
                    MessageBox.Show("Lỗi xóa lớp!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            return;
        }

        private void bbtnRecovery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            bdsLop.CancelEdit();
            bdsLop.Position = vitriSV;
            gcLop.Enabled = true;
            groupBox1.Enabled = false;

            //Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            cmbKhoaInUse.Enabled = true;
        }

        private void tsAdd_Click(object sender, EventArgs e) {
            vitriSV = bdsSV.Position;
            bdsSV.AddNew();
            currentCell = dgvSV.CurrentCell;

            //Button control
            bbtnAdd.Enabled = false;
            bbtnEdit.Enabled = false;
            bbtnRemove.Enabled = false;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = false;
            tsEdit.Enabled = false;
            tsRemove.Enabled = false;
            tsWrite.Enabled = true;
            tsRecovery.Enabled = true;
            tsChangeClass.Enabled = false;

            //View control
            gcLop.Enabled = false;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = false;
            dgvtxbMaLop.ReadOnly = true;

            cmbKhoaInUse.Enabled = false;
            //foreach (DataGridViewRow row in dgvSV.Rows) {
            //    row.ReadOnly = true;
            //}

            for (int i = 0; i < dgvSV.Rows.Count - 1; i++) {
                dgvSV.Rows[i].ReadOnly = true;
            }

            return;
        }

        private void tsEdit_Click(object sender, EventArgs e) {

            dataGridViewMode = 2;
            vitriSV = bdsSV.Position;

            //Button control
            bbtnAdd.Enabled = false;
            bbtnEdit.Enabled = false;
            bbtnRemove.Enabled = false;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = false;
            tsEdit.Enabled = false;
            tsRemove.Enabled = false;
            tsWrite.Enabled = true;
            tsRecovery.Enabled = true;
            tsChangeClass.Enabled = false;

            //View control
            gcLop.Enabled = false;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = false;
            dgvtxbMaLop.ReadOnly = true;

            cmbKhoaInUse.Enabled = false;

            //foreach (DataGridViewRow row in dgvSV.Rows) {
            //    row.Cells["dgvtxbMaSV"].ReadOnly = true;
            //}
            //dgvSV.Columns["dgvtxbMaSV"].DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            //dgvSV.Columns["dgvtxbMaSV"].DefaultCellStyle.BackColor = System.Drawing.Color.Orange;
            dgvSV.Columns["dgvtxbMaSV"].ReadOnly = true;

            return;
        }

        private void tsWrite_Click(object sender, EventArgs e) {
            dataGridViewMode = 0;

            //Validate
            Console.WriteLine(dgvtxbMaSV.ToString());

            try {
                bdsSV.EndEdit();
                //bdsSV.ResetCurrentItem();
                if (dS_SERVER1.HasChanges())
                    taSV.Update(dS_SERVER1.SINHVIEN);
            } catch (Exception err) {
                Console.WriteLine(err.Message);
                if (err.Message.Contains("PRIMARY")) {
                    MessageBox.Show("Mã sinh viên bị trùng!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                } else {
                    MessageBox.Show("Lỗi tạo sinh viên, vui lòng xem lại!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            //----> Write to db success
            //Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = true;
            tsEdit.Enabled = true;
            tsRemove.Enabled = true;
            tsWrite.Enabled = false;
            tsRecovery.Enabled = false;
            tsChangeClass.Enabled = true;

            //View control
            gcLop.Enabled = true;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = true;
            dgvtxbMaLop.ReadOnly = true;

            cmbKhoaInUse.Enabled = true;
            bdsSV.Position = vitriSV;
        }

        private void tsRemove_Click(object sender, EventArgs e) {
            if (bdsDiem.Count > 0) {
                MessageBox.Show("Sinh viên có điểm thi không được xóa!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (bdsHocPhi.Count > 0) {
                MessageBox.Show("Sinh viên đã đóng học phí không được xóa!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Bạn muốn xóa sinh viên này?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes) {
                try {
                    bdsSV.RemoveCurrent();
                    taSV.Update(dS_SERVER1.SINHVIEN);
                    return;
                } catch (Exception err) {
                    Console.WriteLine(err.Message);
                    MessageBox.Show("Lỗi xóa sinh viên!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            return;
        }

        private void tsRecovery_Click(object sender, EventArgs e) {

            bdsSV.CancelEdit();
            if (dataGridViewMode != 2) //Không phải edit
                dgvSV.Rows.Remove(dgvSV.Rows[currentCell.RowIndex]);
            else
                this.taSV.Fill(this.dS_SERVER1.SINHVIEN);

            //dgvSV.Rows[4].Cells[0].Value
            //dgvSV.Refresh();
            //foreach (DataGridViewRow row in dgvSV.Rows) {
            //    if (row.Selected == true)
            //        dgvSV.Rows.Remove(row);
            //}

            foreach (DataGridViewRow row in dgvSV.Rows) {
                foreach (DataGridViewCell cell in row.Cells) {
                    //do operations with cell
                    dgvSV.CurrentCell = dgvSV.Rows[cell.RowIndex].Cells[cell.ColumnIndex];
                    dgvSV.CancelEdit();
                }
            }

            //Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = true;
            tsEdit.Enabled = true;
            tsRemove.Enabled = true;
            tsWrite.Enabled = false;
            tsRecovery.Enabled = false;
            tsChangeClass.Enabled = true;

            //View control
            gcLop.Enabled = true;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = true;
            dgvtxbMaLop.ReadOnly = true;

            cmbKhoaInUse.Enabled = true;
            bdsSV.Position = vitriSV;
        }

        private void tsChangeClass_Click(object sender, EventArgs e) {
            flyoutPanel1.ShowPopup();

            //Không bind dữ liệu, không chuyển sv ra khỏi khoa
            Dictionary<string, string> list = new Dictionary<string, string>();
            for (int i = 0; i < gridView1.DataRowCount; i++) {
                if (!gridView1
                    .GetRowCellValue(i, "MALOP")
                    .ToString()
                    .Equals(txteMaLop.Text.ToString()))
                    list.Add(gridView1.GetRowCellValue(i, "TENLOP").ToString(),
                        gridView1.GetRowCellValue(i, "MALOP").ToString());
            }
            cmbChangeClass.DataSource = list.ToList();
            cmbChangeClass.DisplayMember = "Key";
            cmbChangeClass.ValueMember = "Value";

            //Auto choose first item
            //Không cần trick chọn 1 xong 0 vẫn ok ? 
            cmbChangeClass.SelectedIndex = 0;

            //Button control
            bbtnAdd.Enabled = false;
            bbtnEdit.Enabled = false;
            bbtnRemove.Enabled = false;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = false;
            tsEdit.Enabled = false;
            tsRemove.Enabled = false;
            tsWrite.Enabled = false;
            tsRecovery.Enabled = false;
            tsChangeClass.Enabled = false;

            //View control
            gcLop.Enabled = false;
            groupBox1.Enabled = false;
            dgvSV.Enabled = false;
            dgvSV.ReadOnly = true;
            dgvtxbMaLop.ReadOnly = true;
            cmbKhoaInUse.Enabled = false;
        }

        private void flyoutPanel1_Load(object sender, EventArgs e) {
            btnOk.Enabled = false;
        }

        private void btnOk_Click(object sender, EventArgs e) {

            string maSinhVienChuyenLop = txteMaSV.Text.ToString();
            string maLopMoi = cmbChangeClass.SelectedValue.ToString();

            for (int i = 0; i < dgvSV.Rows.Count; i++) {
                DataGridViewCell cell = dgvSV.Rows[i].Cells["dgvtxbMASV"];
                if (cell.Value.ToString().Equals(maSinhVienChuyenLop)) {
                    dgvSV.Rows[i].Cells["dgvtxbMALOP"].Value = maLopMoi;
                    try {
                        bdsSV.EndEdit();
                        if (dS_SERVER1.HasChanges())
                            taSV.Update(dS_SERVER1.SINHVIEN);
                        this.taSV.Fill(this.dS_SERVER1.SINHVIEN);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                }
            }


            //List<string> listLop = new List<string>();
            //for (int i = 0; i < gridView1.DataRowCount; i++) {
            //    listLop.Add(gridView1.GetRowCellValue(i, "MALOP").ToString());
            //}
            //for (int i = 0; i < dgvSV.Rows.Count; i++) {
            //    DataGridViewCell cell = dgvSV.Rows[i].Cells["dgvtxbMASV"];
            //    if (cell.Value.ToString().Equals(maSinhVienChuyenLop)) {
            //        if (listLop.Contains(maLopMoi)) {
            //            dgvSV.Rows[i].Cells["dgvtxbMALOP"].Value = maLopMoi;
            //        } else {
            //            string sql = $"UPDATE LINK0.QLDSV.dbo.SINHVIEN " +
            //                $"SET MALOP='{maLopMoi}' " +
            //                $"WHERE MASV='{maSinhVienChuyenLop}'";
            //            SqlCommand cmd = new SqlCommand(sql, Program.conn);
            //            cmd.ExecuteNonQuery();
            //        }
            //        try {
            //            bdsSV.EndEdit();
            //            if (dS_SERVER1.HasChanges())
            //                taSV.Update(dS_SERVER1.SINHVIEN);
            //            this.taSV.Fill(this.dS_SERVER1.SINHVIEN);
            //        } catch (Exception ex) {
            //            Console.WriteLine(ex.Message);
            //        }
            //        break;
            //    }
            //}



            //Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = true;
            tsEdit.Enabled = true;
            tsRemove.Enabled = true;
            tsWrite.Enabled = false;
            tsRecovery.Enabled = false;
            tsChangeClass.Enabled = true;

            //View control
            gcLop.Enabled = true;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = true;
            dgvtxbMaLop.ReadOnly = true;

            cmbKhoaInUse.Enabled = true;
            bdsSV.Position = vitriSV;
            flyoutPanel1.HidePopup();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            // Button control
            bbtnAdd.Enabled = true;
            bbtnEdit.Enabled = true;
            bbtnRemove.Enabled = true;
            bbtnWrite.Enabled = false;
            bbtnRecovery.Enabled = false;

            tsAdd.Enabled = true;
            tsEdit.Enabled = true;
            tsRemove.Enabled = true;
            tsWrite.Enabled = false;
            tsRecovery.Enabled = false;
            tsChangeClass.Enabled = true;

            //View control
            gcLop.Enabled = true;
            groupBox1.Enabled = false;
            dgvSV.Enabled = true;
            dgvSV.ReadOnly = true;
            dgvtxbMaLop.ReadOnly = true;

            cmbKhoaInUse.Enabled = true;
            bdsSV.Position = vitriSV;
            flyoutPanel1.HidePopup();
        }

        private void dgvSV_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            string headerText = dgvSV.Columns[e.ColumnIndex].HeaderText;


            //Validation if cell is in the MASV column
            if (headerText.Equals("MASV")) {
                validateStringField("MASV", 10, e, false);
            }

            if (headerText.Equals("HO")) {
                validateStringField("HO", 40, e);
            }

            if (headerText.Equals("TEN")) {
                validateStringField("TEN", 10, e);
            }

            if (headerText.Equals("NGAYSINH")) {
                DateTime dateTime;
                try {
                    dateTime = Convert.ToDateTime(e.FormattedValue.ToString());
                } catch (Exception ex) {
                    MessageBox.Show("Ngày sinh không hợp lệ!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine(ex.Message);
                    e.Cancel = true;
                    return;
                }

                //Cell is not empty
                if (string.IsNullOrEmpty(e.FormattedValue.ToString())) {
                    dgvSV.Rows[e.RowIndex].ErrorText = "NGAYSINH không được rỗng!";
                    e.Cancel = true;
                }

                //Năm sinh <= 1900
                if (dateTime.Year < 1900) {
                    dgvSV.Rows[e.RowIndex].ErrorText = "Năm sinh phải lớn hơn 1900!";
                    e.Cancel = true;
                }
            }

            if (headerText.Equals("NOISINH")) {
                validateStringField("NOISINH", 40, e);
            }

            if (headerText.Equals("DIACHI")) {
                validateStringField("DIACHI", 80, e);
            }
        }

        private void validateStringField(String field, int condition, DataGridViewCellValidatingEventArgs e, bool isNullable = true) {
            //Cell is not empty
            if (!isNullable && string.IsNullOrEmpty(e.FormattedValue.ToString())) {
                dgvSV.Rows[e.RowIndex].ErrorText = $"{field} không được rỗng!";
                MessageBox.Show($"{field} không được rỗng!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }

            if (e.FormattedValue.ToString().Length > condition) {
                dgvSV.Rows[e.RowIndex].ErrorText = $"{field} không được nhiều hơn {condition} ký tự!";
                MessageBox.Show($"{field} không được nhiều hơn {condition} ký tự!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
        }

        private void dgvSV_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            string value = string.Empty;

            dgvSV.Rows[e.RowIndex].ErrorText = string.Empty;
            //value = dgvSV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim();
            //dgvSV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value;

            if (dgvSV.Columns[e.ColumnIndex].HeaderText.Equals("MASV")) {
                value = dgvSV.Rows[e.RowIndex]
                    .Cells[e.ColumnIndex]
                    .Value
                    .ToString()
                    .Trim();
                value = Regex.Replace(value, @"\s+", string.Empty);
                value = Regex.Replace(value, "[^0-9a-zA-Z-]+", string.Empty);
                dgvSV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value;
            }

            if (dgvSV.Columns[e.ColumnIndex].HeaderText.Equals("HO") ||
                dgvSV.Columns[e.ColumnIndex].HeaderText.Equals("TEN")) {
                value = dgvSV.Rows[e.RowIndex]
                    .Cells[e.ColumnIndex]
                    .Value
                    .ToString()
                    .Trim();
                value = Regex.Replace(value, @"\s+", " ");
                value = Regex.Replace(value, "[/\\,;:.`?{}[\\]=+<>@#$%^&*]+", string.Empty);
                dgvSV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value;
            }

            if (dgvSV.Columns[e.ColumnIndex].HeaderText.Equals("NOISINH") ||
                dgvSV.Columns[e.ColumnIndex].HeaderText.Equals("DIACHI")) {
                value = dgvSV.Rows[e.RowIndex]
                    .Cells[e.ColumnIndex]
                    .Value
                    .ToString()
                    .Trim();
                value = Regex.Replace(value, @"\s+", " ");
                value = Regex.Replace(value, "[\\;:`?{}[\\]=+<>@#$%^&*]+", string.Empty);
                dgvSV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value;
            }
        }

        private void dgvSV_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            MessageBox.Show("Error happened " + e.Context.ToString());
        }

        private void cmbKhoaInUse_SelectedIndexChanged(object sender, EventArgs e) {

            if (cmbKhoaInUse.SelectedValue.ToString() != "System.Data.DataRowView") {
                Program.servername = cmbKhoaInUse.SelectedValue.ToString();
            }

            if (cmbKhoaInUse.SelectedIndex != Program.mChinhanh) {
                Program.mlogin = Program.remotelogin;
                Program.password = Program.remotepassword;
            } else {
                Program.mlogin = Program.mloginDN;
                Program.password = Program.passwordDN;
            }

            if (Program.Connect() == 0) {
                MessageBox.Show("Lỗi kết nối khoa mới", "", MessageBoxButtons.OK);
            } else {
                try {
                    this.taSV.Connection.ConnectionString = Program.connstr;
                    this.taLop.Connection.ConnectionString = Program.connstr;
                    this.taKhoa.Connection.ConnectionString = Program.connstr;
                    this.taDiem.Connection.ConnectionString = Program.connstr;
                    this.taHocPhi.Connection.ConnectionString = Program.connstr;

                    // TODO: This line of code loads data into the 'dS_SERVER1.KHOA' table. You can move, or remove it, as needed.
                    this.taKhoa.Fill(this.dS_SERVER1.KHOA);
                    // TODO: This line of code loads data into the 'dS_SERVER1.LOP' table. You can move, or remove it, as needed.
                    this.taLop.Fill(this.dS_SERVER1.LOP);
                    // TODO: This line of code loads data into the 'dS_SERVER1.SINHVIEN' table. You can move, or remove it, as needed.
                    this.taSV.Fill(this.dS_SERVER1.SINHVIEN);
                    // TODO: This line of code loads data into the 'dS_SERVER1.DIEM' table. You can move, or remove it, as needed.
                    this.taDiem.Fill(this.dS_SERVER1.DIEM);
                    // TODO: This line of code loads data into the 'dS_SERVER1.HOCPHI' table. You can move, or remove it, as needed.
                    this.taHocPhi.Fill(this.dS_SERVER1.HOCPHI);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void ClassForm_FormClosing(object sender, FormClosingEventArgs e) {
            Program.servername = tenServer;
        }

        private void ClassForm_FormClosed(object sender, FormClosedEventArgs e) {
            flyoutPanel1.HidePopup();
        }

        private void cmbChangeClass_SelectedIndexChanged(object sender, EventArgs e) {
            int? seleted = cmbChangeClass.SelectedIndex;
            Console.WriteLine(seleted);
            if (seleted != null && seleted != -1)
                btnOk.Enabled = true;
            else
                btnOk.Enabled = false;
        }

        private void cmbChangeClass_KeyUp(object sender, KeyEventArgs e) {
            int? seleted = cmbChangeClass.SelectedIndex;
            Console.WriteLine(seleted);
            if (seleted != null && seleted != -1)
                btnOk.Enabled = true;
            else
                btnOk.Enabled = false;
        }
    }
}