using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Matrix
{
    private float[,] _matrix;
    public int rows { get; }
    public int cols { get; }
    public Matrix(int rows, int cols)
    {
        Debug.Assert(rows > 0 && cols > 0, "Rows and cols musb be larger than 0");

        this.rows = rows;
        this.cols = cols;
        _matrix = new float[rows, cols];
    }
    public Matrix(float[,] val)
    {
        this.rows = val.GetLength(0);
        this.cols = val.GetLength(1);
        _matrix = new float[rows, cols];

        for(int i = 0; i < rows; ++i)
        {
            for(int j = 0; j < cols; ++j)
            {
                _matrix[i, j] = val[i, j];
            }
        }
    }

    public override string ToString()
    {
        string res = "[ ";
        for(int i = 0; i < rows; ++i)
        {
            res += "[ ";
            for(int j = 0; j < cols; ++j)
            {
                res += (_matrix[i, j].ToString() + " ");
            }
            res += "]";
        }
        res += " ]";
        return res;
    }

    public float this[int r, int c]
    {
        get { return _matrix[r, c]; }
        set { _matrix[r, c] = value; }
    }

    public static Matrix operator +(Matrix lhs, Matrix rhs)
    {
        Debug.Assert(lhs.rows == rhs.rows && lhs.cols == rhs.cols, "Columns of LHS and rows of RHS must be same for matrix multiplication");
        Matrix res = new Matrix(lhs.rows, lhs.cols);
        for(int i = 0; i < lhs.rows; ++i)
        {
            for(int j = 0; j < lhs.cols; ++j)
            {
                res[i, j] = lhs[i, j] + rhs[i, j];
            }
        }
        return res;
    }

    public static Matrix operator -(Matrix lhs, Matrix rhs)
    {
        Debug.Assert(lhs.rows == rhs.rows && lhs.cols == rhs.cols, "Columns of LHS and rows of RHS must be same for matrix multiplication");
        Matrix res = new Matrix(lhs.rows, lhs.cols);
        for(int i = 0; i < lhs.rows; ++i)
        {
            for(int j = 0; j < lhs.cols; ++j)
            {
                res[i, j] = lhs[i, j] - rhs[i, j];
            }
        }
        return res;
    }

    public static Matrix operator *(Matrix lhs, Matrix rhs)
    {
        Debug.Assert(lhs.rows == rhs.rows && lhs.cols == rhs.cols, "Columns of LHS and rows of RHS must be same for matrix multiplication");
        Matrix res = new Matrix(lhs.rows, lhs.cols);
        for(int i = 0; i < lhs.rows; ++i)
        {
            for(int j = 0; j < lhs.cols; ++j)
            {
                res[i, j] = lhs[i, j] * rhs[i, j];
            }
        }
        return res;
    }

    public static Matrix operator *(float f, Matrix mat)
    {
        for(int i = 0; i < mat.rows; ++i)
        {
            for(int j = 0; j < mat.cols; ++j)
            {
                mat[i, j] *= f;
            }
        }
        return mat;
    }

    public static Matrix operator /(Matrix lhs, Matrix rhs)
    {
        Debug.Assert(lhs.rows == rhs.rows && lhs.cols == rhs.cols, "Columns of LHS and rows of RHS must be same for matrix multiplication");
        Matrix res = new Matrix(lhs.rows, lhs.cols);
        for(int i = 0; i < lhs.rows; ++i)
        {
            for(int j = 0; j < lhs.cols; ++j)
            {
                res[i, j] = lhs[i, j] / rhs[i, j];
            }
        }
        return res;
    }

    public float[,] GetMatrix()
    {
        return _matrix.Clone() as float[,];
    }

    public Matrix MatMul(Matrix rhs)
    {
        Debug.Assert(this.cols == rhs.rows, "Columns of LHS and rows of RHS must be same for matrix multiplication");
        Matrix res = new Matrix(this.rows, rhs.cols);
        for(int i = 0; i < this.rows; ++i)
        {
            for(int j = 0; j < rhs.cols; ++j)
            {
                float sum = 0.0f;
                for(int k = 0; k < rhs.rows; ++k)
                {
                    sum += this[i, k] * rhs[k, j];
                }
                res[i, j] = sum;
            }
        }
        return res;
    }

    public float Dot(Matrix lhs, Matrix rhs)
    {
        Debug.Assert(lhs.rows == rhs.rows && lhs.cols == rhs.cols);
        float res = 0.0f;
        for(int i = 0; i < lhs.rows; ++i)
        {
            for(int j = 0; j < lhs.cols; ++j)
            {
                res += lhs[i, j] * rhs[i, j];
            }
        }
        return res;
    }

    public float Dot(Matrix rhs)
    {
        Debug.Assert(this.rows == rhs.rows && this.cols == rhs.cols);
        float res = 0.0f;
        for(int i = 0; i < this.rows; ++i)
        {
            for(int j = 0; j < this.cols; ++j)
            {
                res += this[i, j] * rhs[i, j];
            }
        }
        return res;
    }

    public void Normalize()
    {
        float denom = 1.0f / this.Norm();
        this = denom * this;
    }

    public float Norm()
    {
        float sqSum = 0.0f;
        for(int i = 0; i < rows; ++i)
        {
            for(int j = 0; j < cols; ++j)
            {
                sqSum += _matrix[i, j] * _matrix[i, j];
            }
        }
        return Mathf.Sqrt(sqSum);
    }

    public void Randomize(float min, float max)
    {
        for(int i = 0; i < rows; ++i)
        {
            for(int j = 0; j < cols; ++j)
            {
                _matrix[i, j] = Random.Range(min, max);
            }
        }
    }

    public Matrix Transpose()
    {
        Matrix res = new Matrix(_matrix.GetLength(1), _matrix.GetLength(0));
        for(int i = 0; i < res.rows; ++i)
        {
            for(int j = 0; j < res.cols; ++j)
            {
                res[i, j] = _matrix[j, i];
            }
        }
        return res;
    }

    public Matrix Horizontalize()
    {
        Matrix res = new Matrix(1, rows * cols);
        for(int i = 0; i < rows; ++i)
        {
            for(int j = 0; j < cols; ++j)
            {
                res[0, i * cols + j] = _matrix[i, j];
            }
        }
        return res;
    }

    public Matrix Verticalize()
    {
        Matrix res = new Matrix(rows*cols, 1);
        for(int i = 0; i < rows; ++i)
        {
            for(int j = 0; j < cols; ++j)
            {
                res[i*cols + j, 0] = _matrix[i, j];
            }
        }
        return res;
    }

    public void SetDiagonal(float val)
    {
        int k = Mathf.Min(rows, cols);
        for(int i = 0; i < k; ++i)
        {
            _matrix[i, i] = val;
        }
    }

    public List<List<Matrix>> SVD()
    {
        int k = Mathf.Min(rows, cols);
        float eps = 0.01f;

        List<float> svs = new List<float>();
        List<Matrix> us = new List<Matrix>();
        List<Matrix> sigmas = new List<Matrix>();
        List<Matrix> vTs = new List<Matrix>();
        for(int i = 0; i < k; ++i)
        {
            Matrix clone = new Matrix(this._matrix);
            Matrix u, v;
            float singularValue;
            for(int j = 0; j < i; ++j)
            {
                clone -= svs[j] * us[j].MatMul(vTs[j]);
            }

            if(rows > cols)
            {
                v = SVD1d(clone, eps);
                u = this.MatMul(v);
                singularValue = u.Norm();
                u = (1.0f / singularValue) * u;
            }
            else
            {
                u = SVD1d(clone, eps);
                v = this.Transpose().MatMul(u);
                singularValue = v.Norm();
                v = (1.0f / singularValue) * v;
            }

            Matrix sigma = new Matrix(1, 1);
            sigma[0, 0] = singularValue;
            sigmas.Add(sigma);
            us.Add(u);
            vTs.Add(v.Transpose());
            svs.Add(singularValue);
        }

        List<List<Matrix>> res = new List<List<Matrix>>();
        res.Add(us);
        res.Add(sigmas);
        res.Add(vTs);
        return res;
    }

    public Matrix SVD1d(Matrix m, float eps)
    {
        Matrix square;
        if(m.rows > m.cols)
        {
            square = m.Transpose().MatMul(m);
        }
        else
        {
            square = m.MatMul(m.Transpose());
        }

        Matrix x = new Matrix(Mathf.Min(m.rows, m.cols), 1);
        x.Randomize(-1.0f, 1.0f);
        x.Normalize();

        Matrix lastV;
        Matrix currV = x;
        for(int i = 0; i < 1024; ++i)
        {
            lastV = currV;
            currV = square.MatMul(lastV);
            currV.Normalize();

            if(Mathf.Abs(currV.Dot(lastV)) > 1.0f - eps)
            {
                return currV;
            }
        }
        return currV;
    }

    public Matrix Pinv(float eps)
    {
        List<List<Matrix>> svd = this.SVD();
        List<Matrix> us = svd[0];
        List<Matrix> sigmas = svd[1];
        List<Matrix> vTs = svd[2];

        Matrix res = new Matrix(cols, rows);
        string message = "";
        for(int i = 0; i < sigmas.Count; ++i)
        {
            float sigma = sigmas[i][0, 0];
            message += sigma.ToString() + " ";
            if(sigma > eps)
            {
                res += (1.0f / sigma) * vTs[i].Transpose().MatMul(us[i].Transpose());
            }
        }
        return res;
    }
}