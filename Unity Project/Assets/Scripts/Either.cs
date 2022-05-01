
//This class is designed to be either type T or type M. 

public class Either<T,M>
{
    private T left;
    public T Left {get { if (!is_left) {throw new System.Exception("Either is rightsided, left side was being retrieved!");} return left; } private set {left = value;}}
    private M right;
    public M Right {get { if (is_left) {throw new System.Exception("Either is rightsided, left side was being retrieved!");} return right; } private set {right = value;}}

    public bool is_left {get; private set;} = false;

    public Either(T _left) {
        Left = _left;
        is_left = true;
    }

    public Either(M _right) {
        Right = _right;
        is_left = false;
    }

    public override string ToString()
    {
        return is_left ? "left: "+left.ToString() : "right: "+right.ToString();
    }
}
