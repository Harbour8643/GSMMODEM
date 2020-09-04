namespace Harbour.Utils
{
    /// <summary>
    /// 定义日志记录严重性级别
    /// </summary>
    internal enum LogLevel
    {
        /// <summary>
        /// 开发期间用于交互调查的日志。这些日志应该主要包含对调试有用且无长期价值的信息
        /// </summary>
        DEBUG = 1,

        /// <summary>
        /// 跟踪应用程序一般流的日志。这些日志应该具有长期价值。
        /// </summary>
        INFO = 2,

        /// <summary>
        /// 突出显示应用程序流中异常或意外事件的日志，但不会导致应用程序执行停止
        /// </summary>
        WARNIMG = 3,

        /// <summary>
        /// 当前执行流因失败而停止时突出显示的日志。这些应该指示当前活动中的故障，而不是应用程序范围内的故障
        /// </summary>
        ERROR = 4,

        /// <summary>
        /// 描述不可恢复的应用程序或系统崩溃或需要立即关注的灾难性故障的日志
        /// </summary>
        CRITICAL = 5,

        /// <summary>
        /// 不用于写入日志消息。指定日志记录类别不应写入任何消息
        /// </summary>
        NONE = 6,
    }
}
