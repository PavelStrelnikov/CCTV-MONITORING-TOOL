import asyncio
import random
from dataclasses import dataclass, field
from typing import Any, Callable, Coroutine

import httpx


@dataclass
class RetryPolicy:
    max_retries: int = 3
    base_delay: float = 1.0
    max_delay: float = 30.0
    exponential_base: float = 2.0
    retry_on: tuple[type[Exception], ...] = field(default=(
        ConnectionError,
        TimeoutError,
        OSError,
        httpx.ConnectError,
        httpx.ReadTimeout,
        httpx.ConnectTimeout,
    ))

    def delay_for_attempt(self, attempt: int) -> float:
        delay = self.base_delay * (self.exponential_base ** attempt)
        delay = min(delay, self.max_delay)
        jitter = delay * 0.2 * (2 * random.random() - 1)  # noqa: S311
        return max(0, delay + jitter)


async def with_retry(
    func: Callable[..., Coroutine[Any, Any, Any]],
    policy: RetryPolicy,
    *args: Any,
    **kwargs: Any,
) -> Any:
    last_exception: Exception | None = None
    for attempt in range(policy.max_retries + 1):
        try:
            return await func(*args, **kwargs)
        except policy.retry_on as exc:
            last_exception = exc
            if attempt < policy.max_retries:
                delay = policy.delay_for_attempt(attempt)
                await asyncio.sleep(delay)
        except Exception:
            raise
    raise last_exception  # type: ignore[misc]
