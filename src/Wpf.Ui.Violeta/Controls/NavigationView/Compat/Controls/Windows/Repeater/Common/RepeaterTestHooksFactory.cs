namespace Wpf.Ui.Violeta.Controls.Compat;

partial class RepeaterTestHooks
{
    private static RepeaterTestHooks s_testHooks = null!;

    static void EnsureHooks()
    {
        s_testHooks ??= new RepeaterTestHooks();
    }

    public static event TypedEventHandler<object, object> BuildTreeCompleted
    {
        add
        {
            EnsureHooks();
            s_testHooks.m_buildTreeCompleted += value;
        }
        remove
        {
            s_testHooks?.m_buildTreeCompleted -= value;
        }
    }

    static void NotifyBuildTreeCompleted()
    {
        s_testHooks?.NotifyBuildTreeCompletedImpl();
    }
}
