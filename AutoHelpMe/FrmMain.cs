using AutoHelpMe.EventBus;
using AutoHelpMe.Function;
using DevExpress.XtraEditors;
using PInvoke;
using SageTools.Extension;
using System.Text;

namespace AutoHelpMe
{
    public partial class FrmMain : XtraForm
    {
        private TaskHelper _taskHelper;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            #region ����״̬��ʼ��

            btn_Run.Enabled = false;
            txt_����.Text = @"�����������������ʹ�ã��Ͻ��⴫�����Ͻ�������ʹ��������������⣬�뷴���������ߡ�ʹ��ʱ����Թ���Ա������С�";

            #endregion ����״̬��ʼ��

            #region �󶨹��ܿ��ֵ

            var functions = new List<FunctionDto>()
            {
                new() { Text = "����/ҵԭ��/����֮��", ActionName = nameof(Functions.���굥ˢ) },
                new() { Text = "����", ActionName = nameof(Functions.����) },
                new() { Text = "̽������", ActionName = nameof(Functions.̽������) },
                new() { Text = "���ͻ��", ActionName = nameof(Functions.����ͻ��) },
                new() { Text = "����-�һ�", ActionName = nameof(Functions.�����һ�) },
                new() { Text = "�رռӳ�", ActionName = nameof(Functions.�رռӳ�) },
                new() { Text = "��ǰ�����(����)", ActionName = nameof(Functions.�) },
                new() { Text = "����", ActionName = nameof(Functions.����) },
                new() { Text = "�鿨-��ֽʮ��", ActionName = nameof(Functions.���ֽ) },
            };
            list_functions.DataSource = functions;
            list_functions.DisplayMember = "Text";
            list_functions.ValueMember = "ActionName";

            list_�����½�.DataSource = new List<FunctionDto>()
            {
                new() { Text = "�ڶ�ʮ����", ActionName = "28" },
                new() { Text = "�ڶ�ʮ����", ActionName = "27" },
                new() { Text = "�ڶ�ʮ����", ActionName = "26" },
                new() { Text = "�ڶ�ʮ����", ActionName = "25" },
            };
            list_�����½�.DisplayMember = "Text";
            list_�����½�.ValueMember = "ActionName";
            list_�����½�.SelectedIndex = 0;

            #endregion �󶨹��ܿ��ֵ

            #region EventBus

            //��־����
            EventBusHelper.EventAggregator.GetEvent<PrintLogEvent>().Subscribe(log =>
            {
                Invoke(SetLogTextBoxText, log.Item1, log.Item2);
            });
            //���Ҵ��ڶ���
            EventBusHelper.EventAggregator.GetEvent<WindowHandleEvent>().Subscribe(handle =>
            {
                var text = User32.GetWindowText(handle);
                WinHelper.Instance.SetWindowHandle(handle);
                WinHelper.Instance.SetWindowTitle(text);
                btn_Run.Enabled = true;
                Logger.Info($"��ѡ�񴰿ڣ���Ϊ��{text}�����ѡ���ˣ����������ѡ");
            });

            EventBusHelper.EventAggregator.GetEvent<TaskOperateEvent>().Subscribe(operate =>
            {
                switch (operate)
                {
                    case TaskOperateType.Start:
                        {
                            Invoke(SetBtnRun, "ֹͣ", true);
                            Invoke(SetBtnPause, null, true);
                        }
                        break;

                    case TaskOperateType.Stop: //�������
                        Invoke(SetBtnRun, "����", true);
                        Invoke(SetBtnPause, null, false);
                        _taskHelper?.Restore();
                        _taskHelper?.Stop();

                        if (checkBox_�رռӳ�.Checked && list_functions.SelectedValue.ToString() != nameof(Functions.�رռӳ�))
                        {
                            Invoke(ִ�йؼӳ�);
                        }

                        if (GlobalConst.FinishReason == "�������Ʊ��" && GlobalConst.LastTask == nameof(Functions.̽������))
                        {
                            Invoke(ִ�и���ͻ��);
                        }
                        if (GlobalConst.FinishReason == "�������Ʊ��" && GlobalConst.LastTask == nameof(Functions.����ͻ��))
                        {
                            Invoke(ִ�и���ͻ��);
                        }
                        break;

                    case TaskOperateType.NoAuth:
                        Invoke(SetBtnRun, "����", false);
                        Invoke(SetBtnPause, null, false);
                        Invoke(SetBtnLock, false);

                        _taskHelper?.Restore();
                        _taskHelper?.Stop();

                        break;
                }
            });

            #endregion EventBus
        }

        private void btn_����_Click(object sender, EventArgs e)
        {
            var screen = WinHelper.Instance.CaptureWindow();
            pictureBox1.Image = screen;
            var value = list_functions.SelectedValue.ToString();
            if (value.IsNullOrWhiteSpace())
            {
                Logger.Error("����ѡ��Ҫִ�еĹ���");
                return;
            }

            if (btn_Run.Text == @"����")
            {
                btn_Run.Text = @"ֹͣ";
                _taskHelper = TaskHelper.NewInstance();
                _taskHelper.TaskName = value!;

                switch (value)
                {
                    case nameof(Functions.����ͻ��):
                        {
                            if (radio_��ͻ.Checked)
                            {
                                value = nameof(Functions.����ͻ��);
                            }
                            else if (radio_�ͻ.Checked)
                            {
                                value = nameof(Functions.�ͻ��);
                            }
                            else
                            {
                                value = nameof(Functions.����);
                            }
                            _taskHelper.TaskName = value!;
                            Functions.Invoke(value!, _taskHelper, num_ͻ���˴���.Value.ToInt32(), checkBox_ͻ���̱���.Checked, checkBox_ͻ�ƺ�껨.Checked);
                        }
                        break;

                    case nameof(Functions.�����һ�):
                        {
                            Functions.Invoke(value!, _taskHelper, num_��ս����.Value.ToInt32(), checkBox_�����һ������˳�.Checked);
                        }
                        break;

                    case nameof(Functions.̽������):
                        {
                            Functions.Invoke(value!, _taskHelper, num_��ս����.Value.ToInt32(), list_�����½�.SelectedValue.ToInt32(28), radio_����_��ͨ.Checked);
                        }
                        break;

                    case nameof(Functions.���굥ˢ):
                        {
                            if (radio_���굥ˢ.Checked)
                            {
                                value = nameof(Functions.���굥ˢ);
                            }
                            else if (radio_�������.Checked)
                            {
                                value = nameof(Functions.�������);
                            }
                            else
                            {
                                value = nameof(Functions.����˾��);
                            }
                            _taskHelper.TaskName = value!;
                            Functions.Invoke(value!, _taskHelper, num_��ս����.Value.ToInt32());
                        }
                        break;

                    default:
                        {
                            Functions.Invoke(value!, _taskHelper, num_��ս����.Value.ToInt32());
                        }
                        break;
                }
            }
            else
            {
                btn_Run.Enabled = false;
                btn_��ͣ.Enabled = false;
                _taskHelper.Restore();
                _taskHelper.Stop();
            }
        }

        private void btn_����_Click(object sender, EventArgs e)
        {
            Logger.Info("��ʼѡ�񴰿ڣ�����������Ϸ/ģ���������ϣ�Ȼ��������м�(����)");
            WinHelper.Instance.HookMouseMiddleClick();
        }

        private void btn_��ͣ_Click(object sender, EventArgs e)
        {
            if (btn_��ͣ.Text == @"��ͣ")
            {
                Invoke(SetBtnPause, "�ָ�", true);
                Logger.Info("����ͣ");
                _taskHelper.Pause();
            }
            else
            {
                Invoke(SetBtnPause, "��ͣ", true);
                Logger.Info("�ѻָ�");
                _taskHelper.Restore();
            }
        }

        private void btn_��Ȩ_Click(object sender, EventArgs e)
        {
            const string text = "������ʵ����Ȩ����";
            const string caption = "��δʵ��";
            new FrmAuth(caption, text).ShowDialog(this);
        }

        private void btn_����_Click(object sender, EventArgs e)
        {
            XtraMessageBox.Show(this, "��ǰ�������°汾(��ʵ�⹦�ܻ�ûʵ��)", "��ǰ�汾��1.0");
        }

        private void btn_����_Click(object sender, EventArgs e)
        {
            new FrmAbout().ShowDialog(this);
        }
    }
}