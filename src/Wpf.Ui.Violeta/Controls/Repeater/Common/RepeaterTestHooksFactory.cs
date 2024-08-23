namespace Wpf.Ui.Violeta.Controls;

partial class RepeaterTestHooks
{
    private static RepeaterTestHooks s_testHooks;

    static void EnsureHooks()
    {
        if (s_testHooks == null)
        {
            s_testHooks = new RepeaterTestHooks();
        }
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
            if (s_testHooks != null)
            {
                s_testHooks.m_buildTreeCompleted -= value;
            }
        }
    }

    static void NotifyBuildTreeCompleted()
    {
        if (s_testHooks != null)
        {
            s_testHooks.NotifyBuildTreeCompletedImpl();
        }
    }
}
