using System.ComponentModel;

namespace BallastLaneBoard.Domain.TaskManagement;

public enum TaskError
{
    [Description("Title is required.")]
    TitleRequired,

    [Description("Invalid status transition.")]
    InvalidStatusTransition,

    [Description("You are not the owner of this task.")]
    NotOwner,

    [Description("Due date must be in the future.")]
    InvalidDueDate,

    [Description("Description is too long.")]
    DescriptionTooLong
}
