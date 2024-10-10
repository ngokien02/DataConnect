using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BT01_SQL
{
    public partial class Form1 : Form
    {
        string strcon = @"Server=KIENVIP\KIENNGO; Database=QLSV_Kien; Integrated Security = True";
        DataSet ds = new DataSet();
        SqlDataAdapter adpMonHoc, adpKetQua;
        SqlCommandBuilder cmbMonHoc;
        BindingSource bs = new BindingSource();
        int stt = 0;
        public Form1()
        {
            InitializeComponent();
            bs.CurrentChanged += Bs_CurrentChanged;
        }

        private void Bs_CurrentChanged(object sender, EventArgs e)
        {
            STT.Text = (bs.Position + 1) + "/" + bs.Count;
            txtDiemBest.Text = diemBest().ToString();
            txtTongSV.Text = tongSV().ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            khoiTaoDoiTuong();
            docDuLieu();
            mocNoiQuanHe();
            khoiTaoBindingSource();
            lienKetDieuKhien();
            txtDiemBest.Text = diemBest().ToString();
            txtTongSV.Text = tongSV().ToString();
            bindingNavigator1.BindingSource = bs;
        }

        private object tongSV()
        {
            object tsv = ds.Tables["KETQUA"].Compute("count(MaSV)", "MaMH = '" + txtMaMH.Text + "'");
            return Convert.ToInt32(tsv);
        }

        private object diemBest()
        {
            double kq = 0;
            object max = ds.Tables["KETQUA"].Compute("max(Diem)", "MaMH = '" + txtMaMH.Text + "'");
            if (max == DBNull.Value)
            {
                kq = 0;
            }
            else
            {
                kq = Convert.ToDouble(max);
            }
            return kq;
        }

        private void lienKetDieuKhien()
        {
            foreach (Control ctr in this.Controls)
            {
                if (ctr is TextBox && ctr.Name != "txtTongSV" && ctr.Name != "txtDiemBest" && ctr.Name != "STT" && ctr.Name != "txtLoaiMH")
                    ctr.DataBindings.Add("text", bs, ctr.Name.Substring(3), true);
            }
            Binding bdMon = new Binding("text", bs, "LoaiMH", true);
            bdMon.Parse += BdMon_Parse;
            bdMon.Format += BdMon_Format;
            txtLoaiMH.DataBindings.Add(bdMon);
        }

        private void BdMon_Format(object sender, ConvertEventArgs e)
        {
            if (e.Value == DBNull.Value || e.Value == null) return;
            e.Value = (Boolean)e.Value ? "Bắt buộc" : "Tùy chọn";
        }

        private void BdMon_Parse(object sender, ConvertEventArgs e)
        {
            if (e.Value == null) return;
            e.Value = e.Value.ToString().ToUpper() == "BAT BUOC" ? true : false;
        }

        private void khoiTaoBindingSource()
        {
            bs.DataSource = ds;
            bs.DataMember = "MONHOC";
        }

        private void mocNoiQuanHe()
        {
            ds.Relations.Add("FK_MH_KQ", ds.Tables["MONHOC"].Columns["MaMH"], ds.Tables["KETQUA"].Columns["MaMH"]);
            ds.Relations["FK_MH_KQ"].ChildKeyConstraint.DeleteRule = Rule.None;
        }

        private void docDuLieu()
        {
            adpMonHoc.FillSchema(ds, SchemaType.Source, "MONHOC");
            adpMonHoc.Fill(ds, "MONHOC");

            adpKetQua.FillSchema(ds, SchemaType.Source, "KETQUA");
            adpKetQua.Fill(ds, "KETQUA");
        }

        private void btnDau_Click(object sender, EventArgs e)
        {
            bs.MoveFirst();
        }

        private void btnTruoc_Click(object sender, EventArgs e)
        {
            bs.MovePrevious();
        }

        private void btnSau_Click(object sender, EventArgs e)
        {
            bs.MoveNext();
        }

        private void btnCuoi_Click(object sender, EventArgs e)
        {
            bs.MoveLast();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            txtMaMH.ReadOnly = false;
            stt = bs.Position;
            bs.AddNew();
            txtMaMH.Focus();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (!txtMaMH.ReadOnly)
            {
                DataRow rmh = ds.Tables["MONHOC"].Rows.Find(txtMaMH.Text);
                if (rmh != null)
                {
                    MessageBox.Show("Mã MH bị trùng", "Lỗi");
                    txtMaMH.Focus();
                    return;
                }
            }
            txtMaMH.ReadOnly = true;
            bs.EndEdit();
            int n = adpMonHoc.Update(ds, "MONHOC");
            if (n > 0)
                MessageBox.Show("Ghi môn học thành công.");
        }

        private void btnKhong_Click(object sender, EventArgs e)
        {
            bs.CancelEdit();
            txtMaMH.ReadOnly = true;
            bs.Position = stt;
            txtDiemBest.Text = diemBest().ToString();
            txtTongSV.Text = tongSV().ToString();
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DataRow rmh = (bs.Current as DataRowView).Row;
            DataRow[] mangDongLienQuan = rmh.GetChildRows("FK_MH_KQ");
            if (mangDongLienQuan.Length > 0)
            {
                MessageBox.Show("Môn học đã có điểm, không được xóa!");
            }
            else
            {
                DialogResult tl;
                tl = MessageBox.Show("Bạn có chắc chắn xóa môn học này không?(y/n)", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (tl == DialogResult.Yes)
                {
                    bs.RemoveCurrent();
                    int n = adpMonHoc.Update(ds, "MONHOC");
                    if (n > 0)
                        MessageBox.Show("Xoá môn học thành công.");
                }
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult n = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (n == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void khoiTaoDoiTuong()
        {
            adpMonHoc = new SqlDataAdapter("Select * from MONHOC", strcon);
            adpKetQua = new SqlDataAdapter("Select * from KETQUA", strcon);

            cmbMonHoc = new SqlCommandBuilder(adpMonHoc);
        }
    }
}
