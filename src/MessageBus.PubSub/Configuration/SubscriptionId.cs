﻿using Google.Cloud.PubSub.V1;
using Google.Protobuf.WellKnownTypes;

namespace MessageBus.PubSub.Configuration;

public class SubscriptionId(
    string projectId,
    TopicId topicId,
    PubSubConfiguration pubSubConfiguration)
{
    public PubSubConfiguration PubSubConfiguration { get; } = pubSubConfiguration;

    public SubscriptionName SubscriptionName =>
        new(projectId, $"{topicId}.{PubSubConfiguration.Subscription.Sufix}");

    public TopicId TopicId => topicId;

    public SubscriptionName DeadLetterSubscriptionName =>
        new(projectId, $"{topicId}.{PubSubConfiguration.Subscription.Sufix}.dl");

    public TopicName DeadLetter =>
        new(projectId, $"{topicId}.{PubSubConfiguration.Subscription.Sufix}.dl");

    public Topic GetDeadLetterTopic() => new()
    {
        TopicName = DeadLetter,
        MessageRetentionDuration = Duration.FromTimeSpan(
            TimeSpan.FromDays(PubSubConfiguration.Subscription.MessageRetentionDurationDays))
    };

    public Subscription GetDeadLetterSubscription() => new()
    {
        SubscriptionName = DeadLetterSubscriptionName,
        TopicAsTopicName = DeadLetter,
        Detached = false,
        RetainAckedMessages = false,
        AckDeadlineSeconds = PubSubConfiguration.Subscription.AckDeadlineSeconds,
        ExpirationPolicy = new ExpirationPolicy(),
        RetryPolicy = new RetryPolicy
        {
            MinimumBackoff = Duration.FromTimeSpan(TimeSpan.FromSeconds(PubSubConfiguration.Subscription.MinBackoffSeconds)),
            MaximumBackoff = Duration.FromTimeSpan(TimeSpan.FromSeconds(PubSubConfiguration.Subscription.MaxBackoffSeconds))
        }
    };

    public Subscription GetSubscription() => new()
    {
        SubscriptionName = SubscriptionName,
        TopicAsTopicName = topicId.TopicName,
        Detached = false,
        RetainAckedMessages = false,
        AckDeadlineSeconds = PubSubConfiguration.Subscription.AckDeadlineSeconds,
        ExpirationPolicy = new ExpirationPolicy(),
        DeadLetterPolicy = new DeadLetterPolicy
        {
            DeadLetterTopic = DeadLetter.ToString(),
            MaxDeliveryAttempts = PubSubConfiguration.Subscription.MaxDeliveryAttempts
        },
        RetryPolicy = new RetryPolicy
        {
            MinimumBackoff = Duration.FromTimeSpan(TimeSpan.FromSeconds(PubSubConfiguration.Subscription.MinBackoffSeconds)),
            MaximumBackoff = Duration.FromTimeSpan(TimeSpan.FromSeconds(PubSubConfiguration.Subscription.MaxBackoffSeconds))
        }
    };

    public override string ToString() => SubscriptionName.ToString();
}
