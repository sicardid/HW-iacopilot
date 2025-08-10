import pytest
import sys
import os
sys.path.insert(0, os.path.abspath(os.path.dirname(__file__)))
from finance import calculate_compound_interest, calculate_annuity_payment, calculate_internal_rate_of_return

def test_calculate_compound_interest_basic():
    assert calculate_compound_interest(1000, 0.05, 3) == pytest.approx(1157.625)

def test_calculate_compound_interest_zero_periods():
    assert calculate_compound_interest(1000, 0.05, 0) == pytest.approx(1000)

def test_calculate_compound_interest_zero_rate():
    assert calculate_compound_interest(1000, 0, 5) == pytest.approx(1000)

def test_calculate_compound_interest_negative_rate():
    assert calculate_compound_interest(1000, -0.1, 2) == pytest.approx(810)

def test_calculate_compound_interest_large_periods():
    # Test with a large number of periods
    result = calculate_compound_interest(1000, 0.05, 50)
    assert result == pytest.approx(11467.4, rel=1e-3)

def test_calculate_compound_interest_negative_principal():
    # Negative principal should return negative future value
    result = calculate_compound_interest(-1000, 0.05, 3)
    assert result == pytest.approx(-1157.625)



def test_calculate_annuity_payment_one_period():
    # Edge case: only one period
    result = calculate_annuity_payment(1000, 0.05, 1)
    assert result == pytest.approx(1050)

def test_calculate_annuity_payment_negative_principal():
    # Negative principal should return negative payment
    result = calculate_annuity_payment(-1000, 0.05, 3)
    assert result < 0

def test_calculate_annuity_payment_negative_rate():
    # Negative rate (depreciating annuity)
    result = calculate_annuity_payment(1000, -0.05, 3)
    assert isinstance(result, float)


def test_calculate_annuity_payment_basic():
    result = calculate_annuity_payment(1000, 0.05, 3)
    assert result == pytest.approx(367.208, abs=1e-3)

def test_calculate_annuity_payment_zero_rate():
    assert calculate_annuity_payment(1000, 0, 4) == pytest.approx(250)



def test_calculate_internal_rate_of_return_empty():
    # Empty cash flows should return 0 or handle gracefully
    irr = calculate_internal_rate_of_return([])
    assert isinstance(irr, float)

def test_calculate_internal_rate_of_return_single_cashflow():
    # Single cash flow should return 0 or handle gracefully
    irr = calculate_internal_rate_of_return([-1000])
    assert isinstance(irr, float)

def test_calculate_internal_rate_of_return_long_series():
    # Long series to test stability
    cash_flows = [-1000] + [100]*20
    irr = calculate_internal_rate_of_return(cash_flows)
    assert isinstance(irr, float)

def test_calculate_internal_rate_of_return_basic():
    # IRR for [-1000, 400, 400, 400] should be close to 0.095 (9.5%)
    irr = calculate_internal_rate_of_return([-1000, 400, 400, 400])
    assert irr == pytest.approx(0.095, abs=1e-2)

def test_calculate_internal_rate_of_return_zero():
    # IRR for [-100, 100] should be 0
    irr = calculate_internal_rate_of_return([-100, 100])
    assert irr == pytest.approx(0, abs=1e-4)
