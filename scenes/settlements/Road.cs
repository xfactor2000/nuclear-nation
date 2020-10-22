using System;
using Godot;
using Godot.Collections;
using Array = System.Array;

namespace nuclearnation
{
    public class Road:Reference
    {
        private readonly Vector2 from;
        private readonly Vector2 to;

        public bool IsVertical()
        {
            return (int)from.x == (int)to.x;
        }

        public Road(Vector2 from, Vector2 to)
        {
            this.from = from;
            this.to = to;
        }

        public Array<Vector2> GetTiles()
        {
            var to_x = to.x;
            if ((int)to_x == (int)from.x)
            {
                to_x += 1;
            }
            
            var to_y = to.y;
            if ((int)to_y == (int)from.y)
            {
                to_y += 1;
            }
            
            var response = new Array<Vector2>();
            for (var x = @from.x; x < to_x; x++)
            {
                for (var y = @from.y; y < to_y; y++)
                {
                    response.Add(new Vector2(x,y));
                }
            }

            return response;
        }


    }
}