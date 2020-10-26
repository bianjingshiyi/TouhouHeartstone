//using UI;
//using UnityEngine;
//using TouhouHeartstone;
//using Card = TouhouCardEngine.Card;
//using System.Linq;
//using BJSYGameCore.UI;

//namespace Game
//{
//    class PropColor : EventAnimation<Card.SetPropEventArg>
//    {
//        AnimAnim _anim;
//        SimpleAnim simpleAnim = null;
//        Animator animator = null;
//        public override bool update(TableManager table, Card.SetPropEventArg eventArg)
//        {
            

//            Card[] cards = table.game.getAllCharacters();
//            foreach(Card c in cards)
//            {
//                if (table.tryGetAllPropNumber(c, out var numbers))
//                {
//                    animator = numbers[0].animator;
//                    //生命值
//                    if (c.getCurrentLife(table.game) < c.getLife(table.game))
//                    {
//                        simpleAnim = numbers[0].RedText;
//                    }
//                    else if (c.getLife(table.game) > c.define.getLife())
//                    {
//                        simpleAnim = numbers[0].GreenText;
//                    }
//                    else
//                    {
//                        simpleAnim = numbers[0].WhiteText;
//                    }

//                    //攻击力
//                    if (c.getAttack(table.game) > c.define.getAttack())
//                    {
//                        simpleAnim = numbers[1].GreenText;
//                    }
//                    else
//                    {
//                        simpleAnim = numbers[1].WhiteText;
//                    }

//                }
//            }
            

//            //else
//            //    throw new ActorNotFoundException(eventArg.card);
//            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator))
//                return false;
//            return true;
//        }

//    }
//}