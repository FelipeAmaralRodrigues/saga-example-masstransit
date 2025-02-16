﻿using MassTransit;
using MediatR;
using StudentProject.Contracts;
using StudentProject.Domain.Mediator;
using StudentProject.Domain.Mediator.Notifications;
using StudentProject.Domain.Students.Commands;

namespace StudentProject.Services.Bus.Consumer.Consumers
{
    public class UpdateStudentThirdPartyUIdConsumer : IConsumer<UpdateStudentThirdPartyUId>
    {
        private readonly IMediatorHandler _mediator;
        private readonly DomainNotificationHandler _notifications;

        public UpdateStudentThirdPartyUIdConsumer(IMediatorHandler mediator, INotificationHandler<DomainNotification> notifications)
        {
            _mediator = mediator;
            _notifications = (DomainNotificationHandler)notifications;
        }

        public async Task Consume(ConsumeContext<UpdateStudentThirdPartyUId> context)
        {
            try
            {
                var command = new UpdateStudentThirdPartyUIdCommand 
                {
                    StudentUId = context.Message.StudentUId,
                    ThirdPartyUId = context.Message.ThirdPartyUId
                };

                await _mediator.SendCommand(command, context.CancellationToken);

                if (_notifications.HasNotifications())
                {
                    await context.Publish(new UpdateStudentThirdPartyUIdValidationFailed
                    {
                        StudentUId = context.Message.StudentUId,
                        ThirdPartyUId = context.Message.ThirdPartyUId,
                        ValidationErrors = _notifications.GetNotifications().ToDictionary(m => m.Key, m => m.Value)
                    });
                }
                else 
                {
                    await context.Publish(new StudentThirdPartyUIdUpdated
                    {
                        RequestUId = context.Message.StudentUId,
                        StudentUId = context.Message.StudentUId,
                        ThirdPartyUId = context.Message.ThirdPartyUId
                    });
                }
            }
            catch (Exception e)
            {
                await context.Publish(new UpdateStudentThirdPartyUIdFailed
                {
                    StudentUId = context.Message.StudentUId,
                    ThirdPartyUId = context.Message.ThirdPartyUId,

                    ExceptionMessage = e.Message,
                    ExceptionStackTrace = e.StackTrace,
                    ExceptionType = e.GetType().ToString()
                });
            }
        }
    }
}
