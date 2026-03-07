import pytest
from unittest.mock import AsyncMock
from cctv_monitor.core.retry import RetryPolicy, with_retry


@pytest.fixture
def policy():
    return RetryPolicy(max_retries=3, base_delay=0.01, max_delay=0.1)


async def test_no_retry_on_success(policy):
    func = AsyncMock(return_value="ok")
    result = await with_retry(func, policy)
    assert result == "ok"
    assert func.call_count == 1


async def test_retry_on_connection_error(policy):
    func = AsyncMock(side_effect=[ConnectionError, ConnectionError, "ok"])
    result = await with_retry(func, policy)
    assert result == "ok"
    assert func.call_count == 3


async def test_raises_after_max_retries(policy):
    func = AsyncMock(side_effect=ConnectionError("fail"))
    with pytest.raises(ConnectionError):
        await with_retry(func, policy)
    assert func.call_count == 4  # initial + 3 retries


async def test_no_retry_on_value_error(policy):
    func = AsyncMock(side_effect=ValueError("bad input"))
    with pytest.raises(ValueError):
        await with_retry(func, policy)
    assert func.call_count == 1


async def test_retry_on_timeout_error(policy):
    func = AsyncMock(side_effect=[TimeoutError, "ok"])
    result = await with_retry(func, policy)
    assert result == "ok"
    assert func.call_count == 2


async def test_retry_on_os_error(policy):
    func = AsyncMock(side_effect=[OSError, "ok"])
    result = await with_retry(func, policy)
    assert result == "ok"
    assert func.call_count == 2


async def test_delay_calculation():
    policy = RetryPolicy(max_retries=3, base_delay=1.0, max_delay=10.0, exponential_base=2.0)
    # attempt 0: 1.0 * 2^0 = 1.0 (± jitter)
    # attempt 1: 1.0 * 2^1 = 2.0 (± jitter)
    # attempt 2: 1.0 * 2^2 = 4.0 (± jitter)
    d0 = policy.delay_for_attempt(0)
    d1 = policy.delay_for_attempt(1)
    d2 = policy.delay_for_attempt(2)
    assert 0.5 < d0 < 1.5  # ~1.0 with jitter
    assert 1.0 < d1 < 3.0  # ~2.0 with jitter
    assert 2.5 < d2 < 5.5  # ~4.0 with jitter


async def test_delay_respects_max():
    policy = RetryPolicy(max_retries=3, base_delay=1.0, max_delay=2.0, exponential_base=10.0)
    d = policy.delay_for_attempt(5)
    assert d <= 2.5  # max_delay + jitter
