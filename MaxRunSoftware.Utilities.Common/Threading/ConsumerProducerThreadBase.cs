﻿// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MaxRunSoftware.Utilities.Common;

public abstract class ConsumerProducerThreadBase<TConsume, TProduce> : ConsumerThreadBase<TConsume>
{
    private readonly BlockingCollection<TProduce> producerQueue;
    private readonly CancellationTokenSource cancellation = new();

    protected ConsumerProducerThreadBase(BlockingCollection<TConsume> consumerQueue, BlockingCollection<TProduce> producerQueue) : base(consumerQueue)
    {
        this.producerQueue = producerQueue.CheckNotNull(nameof(producerQueue));
    }

    protected override void WorkConsume(TConsume item)
    {
        if (IsCancelled) return;

        try
        {
            var produceItem = WorkConsumeProduce(item);
            if (IsCancelled) return;

            producerQueue.Add(produceItem, cancellation.Token);
        }
        catch (OperationCanceledException) { Cancel(); }
    }

    protected override void CancelInternal()
    {
        try { cancellation.Cancel(); }
        catch (Exception e)
        {
            log.LogWarning(e, "CancellationTokenSource.Cancel() request threw exception");
        }

        base.CancelInternal();
    }

    protected abstract TProduce WorkConsumeProduce(TConsume item);
}
