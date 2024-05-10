#include <iostream>

#include <precision.h>
#include <precision_ext.h>

#include "precision_tests.h"

static unsigned int SUCCEEDED = 0;
static unsigned int FAILED = 0;

#define CHECK(condition, message) if(condition) { std::cout << "DONE: " << message << std::endl; SUCCEEDED++; } else { std::cerr << "FAILED: " << message << std::endl; FAILED++; }

void test_orient2d()
{
    Vec2 a = { -0.5f, -0.5f };
    Vec2 b = { 0.5f, -0.5f };
    Vec2 c = { 0.0f, 1.0f };
    CHECK(orient2d(&a, &b, &c) > 0, "orient2d check");
}

void test_exactsign()
{
    CHECK(exactsign(1.0f) == 1, "1.0f is positiv");
    CHECK(exactsign(-1.0f) == -1, "-1.0f is negative");
    CHECK(exactsign(0.0f) == 0, "0.0f is exact zero");
}

int main()
{
    const REAL eps = exactinit();
    std::cout << "Running tests with normative eps = " << eps << std::endl;

    test_orient2d();
    test_exactsign();

    std::cout << diff(0.24f + eps, 0.24f) << std::endl;

    std::cout << "Total test done: " << SUCCEEDED + FAILED << std::endl;
    std::cout << " succeeded     : " << SUCCEEDED << std::endl;
    std::cout << " failed        : " << FAILED << std::endl;
}