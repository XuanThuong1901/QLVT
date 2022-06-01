using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace QLVT
{
    public partial class FormDangNhap : DevExpress.XtraEditors.XtraForm
    {



        //mởi kết nối Publisher
        private SqlConnection connPublisher = new SqlConnection();

        //lấy danh sách phân mảnh
        private void layDanhSachPhanManh(String cmd)
        {
            if (connPublisher.State == ConnectionState.Closed)
            {
                connPublisher.Open();
            }
            DataTable dt = new DataTable();
            // adapter dùng để đưa dữ liệu từ view sang database
            SqlDataAdapter da = new SqlDataAdapter(cmd, connPublisher);
            // dùng adapter thì mới đổ vào data table được
            da.Fill(dt);
            connPublisher.Close();
            Program.bindingSource.DataSource = dt;

            cmbCHINHANH.DataSource = Program.bindingSource;
            cmbCHINHANH.DisplayMember = "TENCN";
            cmbCHINHANH.ValueMember = "TENSERVER";
        }

        //check form exist trách nhấn  2 lần
        private Form CheckExists(Type ftype)
        {
            foreach (Form f in this.MdiChildren)
                if (f.GetType() == ftype)
                    return f;
            return null;
        }


        //kết nối server 
        private int KetNoiDatabaseGoc()
        {
            if (connPublisher != null && connPublisher.State == ConnectionState.Open)
                connPublisher.Close();
            try
            {
                connPublisher.ConnectionString = Program.connstrPublisher;
                connPublisher.Open();
                return 1;
            }

            catch (Exception e)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu.\nBạn xem lại user name và password.\n " + e.Message, "", MessageBoxButtons.OK);
                return 0;
            }
        }




        public FormDangNhap()
        {
            InitializeComponent();
        }

        private void btnDANGNHAP_Click(object sender, EventArgs e)
        {
            //Kiem tra dữ liệu đầu vào
            if (txtTAIKHOAN.Text.Trim() == "" || txtMATKHAU.Text.Trim() == "")
            {
                MessageBox.Show("Tài khoản & mật khẩu không thể bỏ trống", "Thông Báo", MessageBoxButtons.OK);
                return;
            }

            //Lấy dữ liệu
            Program.loginName = txtTAIKHOAN.Text.Trim();
            Program.loginPassword = txtMATKHAU.Text.Trim();
            
            //Kiểm tra kết nôi
            if (Program.KetNoi() == 0)
                return;

            //lấy mã chi nhánh
            // ví dụ current LT là loginName của server 1 thì qua server 2 dùng HTKN.
            //*Quay về server 1 thì dùng currentLogin
            Program.brand = cmbCHINHANH.SelectedIndex;
            Program.currentLogin = Program.loginName;
            Program.currentPassword = Program.loginPassword;


            // chạy Sp kiểm tra đăng nhập
            String statement = "EXEC sp_DangNhap '" + Program.loginName + "'";// exec sp_DangNhap 'TP'
            Program.myReader = Program.ExecSqlDataReader(statement);
            if (Program.myReader == null)
                return;
            // đọc một dòng của myReader - điều này là hiển nhiên vì kết quả chỉ có 1 dùng duy nhất
            Program.myReader.Read();


            //lấy username -> suy ra quyền, mã nhân viên gán ở gốc màn hình
            Program.userName = Program.myReader.GetString(0);// lấy userName
            if (Convert.IsDBNull(Program.userName))
            {
                MessageBox.Show("Tài khoản này không có quyền truy cập \n Hãy thử tài khoản khác", "Thông Báo", MessageBoxButtons.OK);
            }


            Program.staff = Program.myReader.GetString(1);
            Program.role = Program.myReader.GetString(2);

            Program.myReader.Close();
            Program.conn.Close();


            Program.formChinh.MANHANVIEN.Text = "MÃ NHÂN VIÊN: " + Program.userName;
            Program.formChinh.HOTEN.Text = "HỌ TÊN: " + Program.staff;
            Program.formChinh.NHOM.Text = "VAI TRÒ: " + Program.role;

            this.Visible = false;
            Program.formChinh.enableButtons();

        }

        private void btnTHOAT_Click(object sender, EventArgs e)
        {
            this.Close();
            Program.formChinh.Close();
        }

        private void FormDangNhap_Load(object sender, EventArgs e)
        {
            // đặt sẵn mật khẩu để đỡ nhập lại nhiều lần
            txtTAIKHOAN.Text = "LT";// nguyen long - chi nhanh
            txtMATKHAU.Text = "123";
            if (KetNoiDatabaseGoc() == 0)
                return;
            //Lấy 2 cái đầu tiên của danh sách
            layDanhSachPhanManh("SELECT TOP 2 * FROM view_DanhSachPhanManh");
            cmbCHINHANH.SelectedIndex = 0;
            cmbCHINHANH.SelectedIndex = 1;
        }

        private void cmbCHINHANH_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Program.serverName = cmbCHINHANH.SelectedValue.ToString();
                //Console.WriteLine(cmbCHINHANH.SelectedValue.ToString());
            }
            catch (Exception)
            {

            }
        }
    }
}