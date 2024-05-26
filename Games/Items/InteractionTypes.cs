namespace WibboEmulator.Games.Items;

public class InteractionTypes
{
    public static InteractionType GetTypeFromString(string pType) => pType switch
    {
        "default" => InteractionType.NONE,
        "wired_item" => InteractionType.WIRED_ITEM,
        "gate" => InteractionType.GATE,
        "photo" => InteractionType.PHOTO,
        "postit" => InteractionType.POSTIT,
        "dimmer" => InteractionType.MOODLIGHT,
        "trophy" => InteractionType.TROPHY,
        "bed" => InteractionType.BED,
        "scoreboard" => InteractionType.SCORE_BOARD,
        "vendingmachine" => InteractionType.VENDING_MACHINE,
        "vendingenablemachine" => InteractionType.VENDING_ENABLE_MACHINE,
        "alert" => InteractionType.ALERT,
        "onewaygate" => InteractionType.ONEWAYGATE,
        "loveshuffler" => InteractionType.LOVESHUFFLER,
        "habbowheel" => InteractionType.HABBOWHEEL,
        "dice" => InteractionType.DICE,
        "bottle" => InteractionType.BOTTLE,
        "teleport" => InteractionType.TELEPORT,
        "teleportfloor" => InteractionType.TELEPORT_ARROW,
        "pet" => InteractionType.PET,
        "pool" => InteractionType.POOL,
        "roller" => InteractionType.ROLLER,
        "fbgate" => InteractionType.FOOTBALL_GATE,
        "iceskates" => InteractionType.ICESKATES,
        "normalskates" => InteractionType.NORMSLASKATES,
        "lowpool" => InteractionType.LOWPOOL,
        "haloweenpool" => InteractionType.HALLOWEENPOOL,
        "football" => InteractionType.FOOTBALL,
        "green_goal" => InteractionType.FOOTBALL_GOAL_GREEN,
        "yellow_goal" => InteractionType.FOOTBALL_GOAL_YELLOW,
        "red_goal" => InteractionType.FOOTBALL_GOAL_RED,
        "blue_goal" => InteractionType.FOOTBALL_GOAL_BLUE,
        "footballcountergreen" => InteractionType.FOOTBALL_COUNTER_GREEN,
        "footballcounteryellow" => InteractionType.FOOTBALL_COUNTER_YELLOW,
        "footballcounterblue" => InteractionType.FOOTBALL_COUNTER_BLUE,
        "footballcountered" => InteractionType.FOOTBALL_COUNTER_RED,
        "bb_blue_gate" => InteractionType.BANZAI_GATE_BLUE,
        "bb_red_gate" => InteractionType.BANZAI_GATE_RED,
        "bb_yellow_gate" => InteractionType.BANZAI_GATE_YELLOW,
        "bb_green_gate" => InteractionType.BANZAI_GATE_GREEN,
        "bb_patch" => InteractionType.BANZAI_FLOOR,
        "banzaiscoreblue" => InteractionType.BANZAI_SCORE_BLUE,
        "banzaiscorered" => InteractionType.BANZAI_SCORE_RED,
        "banzaiscoreyellow" => InteractionType.BANZAI_SCORE_YELLOW,
        "banzaiscoregreen" => InteractionType.BANZAI_SCORE_GREEN,
        "bb_teleport" => InteractionType.BANZAI_TELE,
        "banzaipuck" => InteractionType.BANZAI_PUCK,
        "banzaipyramid" => InteractionType.BANZAI_PYRAMID,
        "wf_blob2" => InteractionType.BANZAI_BLOB_2,
        "wf_blob" => InteractionType.BANZAI_BLOB,
        "counter" => InteractionType.CHRONO_TIMER,
        "freezeexit" => InteractionType.FREEZE_EXIT,
        "freezeredcounter" => InteractionType.FREEZE_RED_COUNTER,
        "freezebluecounter" => InteractionType.FREEZE_BLUE_COUNTER,
        "freezeyellowcounter" => InteractionType.FREEZE_YELLOW_COUNTER,
        "freezegreencounter" => InteractionType.FREEZE_GREEN_COUNTER,
        "freezeyellowgate" => InteractionType.FREEZE_YELLOW_GATE,
        "freezeredgate" => InteractionType.FREEZE_RED_GATE,
        "freezegreengate" => InteractionType.FREEZE_GREEN_GATE,
        "freezebluegate" => InteractionType.FREEZE_BLUE_GATE,
        "freezetileblock" => InteractionType.FREEZE_TILE_BLOCK,
        "freezetile" => InteractionType.FREEZE_TILE,
        "jukebox" => InteractionType.JUKEBOX,
        "wf_trg_attime" => InteractionType.TRIGGER_ONCE,
        "wf_trg_collision" => InteractionType.TRIGGER_COLLISION,
        "wf_trg_enterroom" => InteractionType.TRIGGER_AVATAR_ENTERS_ROOM,
        "wf_trg_gameend" => InteractionType.TRIGGER_GAME_ENDS,
        "wf_trg_gamestart" => InteractionType.TRIGGER_GAME_STARTS,
        "wf_trg_timer" => InteractionType.TRIGGER_PERIODICALLY,
        "wf_trg_period_long" => InteractionType.TRIGGER_PERIODICALLY_LONG,
        "wf_trg_onsay" => InteractionType.TRIGGER_AVATAR_SAYS_SOMETHING,
        "wf_trg_cmd" => InteractionType.TRIGGER_COMMAND,
        "wf_trg_cls_user" => InteractionType.TRIGGER_COLLISION_USER,
        "wf_trg_user_clicked" => InteractionType.TRIGGER_USER_CLICK,
        "wf_trg_user_click" => InteractionType.TRIGGER_USER_CLICK_SELF,
        "wf_trg_cls_user_self" => InteractionType.TRIGGER_COLLISION_USER_SELF,
        "wf_trg_atscore" => InteractionType.TRIGGER_SCORE_ACHIEVED,
        "wf_trg_furnistate" => InteractionType.TRIGGER_STATE_CHANGED,
        "wf_trg_onfurni" => InteractionType.TRIGGER_WALK_ON_FURNI,
        "wf_trg_offfurni" => InteractionType.TRIGGER_WALK_OFF_FURNI,
        "wf_trg_exit_room" => InteractionType.TRIGGER_AVATAR_EXIT,
        "wf_act_givepoints" => InteractionType.ACTION_GIVE_SCORE,
        "wf_act_matchfurni" => InteractionType.ACTION_POS_RESET,
        "wf_act_moverotate" => InteractionType.ACTION_MOVE_ROTATE,
        "wf_act_reset_timers" => InteractionType.ACTION_RESET_TIMER,
        "wf_act_saymsg" => InteractionType.ACTION_SHOW_MESSAGE,
        "wf_act_give_reward" => InteractionType.ACTION_GIVE_REWARD,
        "superwired" => InteractionType.ACTION_SUPER_WIRED,
        "superwiredcondition" => InteractionType.CONDITION_SUPER_WIRED,
        "wf_act_moveuser" => InteractionType.ACTION_TELEPORT_TO,
        "wf_act_endgame_team" => InteractionType.ACTION_ENDGAME_TEAM,
        "wf_act_call_stacks" => InteractionType.ACTION_CALL_STACKS,
        "wf_act_togglefurni" => InteractionType.ACTION_TOGGLE_STATE,
        "wf_act_kick_user" => InteractionType.ACTION_KICK_USER,
        "wf_act_flee" => InteractionType.ACTION_FLEE,
        "wf_act_chase" => InteractionType.ACTION_CHASE,
        "wf_act_collisioncase" => InteractionType.ACTION_COLLISION_CASE,
        "wf_act_collisionitem" => InteractionType.ACTION_COLLISION_ITEM,
        "wf_act_collisionteam" => InteractionType.ACTION_COLLISION_TEAM,
        "wf_act_move_to_dir" => InteractionType.ACTION_MOVE_TO_DIR,
        "wf_act_room_message" => InteractionType.ACTION_ROOM_MESSAGE,
        "wf_act_teleport_furni" => InteractionType.ACTION_TELEPORT_FURNI,
        "wf_act_tridimension" => InteractionType.ACTION_TRIDIMENSION,
        "wf_cnd_furnis_hv_avtrs" => InteractionType.CONDITION_FURNIS_HAVE_USERS,
        "wf_cnd_furnis_hv_prson" or "wf_cnd_not_hv_avtrs" => InteractionType.CONDITION_FURNIS_HAVE_NO_USERS,
        "conditionstatepos" => InteractionType.CONDITION_STATE_POS,
        "wf_cnd_stuff_is" => InteractionType.CONDITION_STUFF_IS,
        "wf_cnd_not_stuff_is" => InteractionType.CONDITION_NOT_STUFF_IS,
        "wf_cnd_not_collision_is" => InteractionType.CONDITION_NOT_COLLISION_IS,
        "wf_cnd_collision_is" => InteractionType.CONDITION_COLLISION_IS,
        "wf_cnd_not_match_snap" => InteractionType.CONDITION_STATE_POS_NEGATIVE,
        "conditiontimelessthan" => InteractionType.CONDITION_TIME_LESS_THAN,
        "conditiontimemorethan" => InteractionType.CONDITION_TIME_MORE_THAN,
        "wf_cnd_trggrer_on_frn" => InteractionType.CONDITION_TRIGGER_ON_FURNI,
        "wf_cnd_not_trggrer_on" => InteractionType.CONDITION_TRIGGER_ON_FURNI_NEGATIVE,
        "wf_cnd_has_furni_on" => InteractionType.CONDITION_HAS_FURNI_ON_FURNI,
        "wf_cnd_not_furni_on" => InteractionType.CONDITION_HAS_FURNI_ON_FURNI_NEGATIVE,
        "wf_cnd_date_rng_active" => InteractionType.CONDITION_DATE_RNG_ACTIVE,
        "floorswitch" => InteractionType.FLOOR_SWITCH,
        "wf_xtra_random" => InteractionType.SPECIAL_RANDOM,
        "wf_xtra_unseen" => InteractionType.SPECIAL_UNSEEN,
        "wf_xtra_animate" => InteractionType.SPECIAL_ANIMATE,
        "wf_xtra_or_eval" => InteractionType.SPECIAL_OR_EVAL,
        "puzzlebox" => InteractionType.PUZZLE_BOX,
        "gift" => InteractionType.GIFT,
        "gift_banner" => InteractionType.GIFT_BANNER,
        "troc_banner" => InteractionType.TROC_BANNER,
        "ltd_birthday_2024" => InteractionType.LTD_BIRTHDAY_2024,
        "extrabox" => InteractionType.EXTRA_BOX,
        "deluxebox" => InteractionType.DELUXE_BOX,
        "legendbox" => InteractionType.LEGEND_BOX,
        "badgebox" => InteractionType.BADGE_BOX,
        "lootbox2022" => InteractionType.LOOTBOX_2022,
        "maniqui" => InteractionType.MANNEQUIN,
        "bgupdater" => InteractionType.TONER,
        "bot" => InteractionType.BOT,
        "adsbackground" => InteractionType.ADS_BACKGROUND,
        "badge_display" => InteractionType.BADGE_DISPLAY,
        "badge_troc" => InteractionType.BADGE_TROC,
        "tvyoutube" => InteractionType.TV_YOUTUBE,
        "pilemagic" => InteractionType.PILE_MAGIC,
        "jmphorse" => InteractionType.HORSE_JUMP,
        "groupfurni" => InteractionType.GUILD_ITEM,
        "groupgate" => InteractionType.GUILD_GATE,
        "floor" => InteractionType.FLOOR,
        "wallpaper" => InteractionType.WALLPAPER,
        "landscape" => InteractionType.LANDSCAPE,
        "wf_cnd_actor_in_group" => InteractionType.CONDITION_ACTOR_IN_GROUP,
        "wf_cnd_not_in_group" => InteractionType.CONDITION_NOT_IN_GROUP,
        "pressure_pad" => InteractionType.PRESSURE_PAD,
        "highscore" => InteractionType.HIGH_SCORE,
        "hightscorepoints" => InteractionType.HIGH_SCORE_POINTS,
        "wf_trg_bot_reached_stf" => InteractionType.TRIGGER_BOT_REACHED_STF,
        "wf_trg_bot_reached_avtr" => InteractionType.TRIGGER_BOT_REACHED_AVTR,
        "wf_act_bot_clothes" => InteractionType.ACTION_BOT_CLOTHES,
        "wf_trg_trigger_self" => InteractionType.TRIGGER_COMMAND_SELF,
        "wf_act_bot_teleport" => InteractionType.ACTION_BOT_TELEPORT,
        "wf_act_bot_follow_avatar" => InteractionType.ACTION_BOT_FOLLOW_AVATAR,
        "wf_act_bot_give_handitem" => InteractionType.ACTION_BOT_GIVE_HANDITEM,
        "wf_act_bot_move" => InteractionType.ACTION_BOT_MOVE,
        "wf_act_user_move" => InteractionType.ACTION_USER_MOVE,
        "wf_act_bot_talk_to_avatar" => InteractionType.ACTION_BOT_TALK_TO_AVATAR,
        "wf_act_bot_talk" => InteractionType.ACTION_BOT_TALK,
        "wf_cnd_has_handitem" => InteractionType.CONDITION_HAS_HANDITEM,
        "wf_act_join_team" => InteractionType.ACTION_JOIN_TEAM,
        "wf_act_leave_team" => InteractionType.ACTION_LEAVE_TEAM,
        "wf_act_give_score_tm" => InteractionType.ACTION_GIVE_SCORE_TM,
        "wf_cnd_actor_in_team" => InteractionType.CONDITION_ACTOR_IN_TEAM,
        "wf_cnd_not_in_team" => InteractionType.CONDITION_NOT_IN_TEAM,
        "wf_cnd_not_user_count" => InteractionType.CONDITION_NOT_USER_COUNT,
        "wf_cnd_user_count_in" => InteractionType.CONDITION_USER_COUNT_IN,
        "wf_act_give_points_highscore" => InteractionType.ACTION_GIVE_POINTS_HIGHSCORE,
        "wf_act_reset_points_highscore" => InteractionType.ACTION_RESET_POINTS_HIGHSCORE,
        "wf_cnd_compare_highscore" => InteractionType.CONDITION_COMPARE_HIGHSCORE,
        "crackable" => InteractionType.CRACKABLE,
        "lovelock" => InteractionType.LOVELOCK,
        "exchange" => InteractionType.EXCHANGE,
        "exchange_tree" => InteractionType.EXCHANGE_TREE,
        "exchange_tree_classic" => InteractionType.EXCHANGE_TREE_CLASSIC,
        "exchange_tree_epic" => InteractionType.EXCHANGE_TREE_EPIC,
        "exchange_tree_legend" => InteractionType.EXCHANGE_TREE_LEGEND,
        "premium_classic" => InteractionType.PREMIUM_CLASSIC,
        "premium_epic" => InteractionType.PREMIUM_EPIC,
        "premium_legend" => InteractionType.PREMIUM_LEGEND,
        "horse_saddle_1" => InteractionType.HORSE_SADDLE_1,
        "horse_saddle_2" => InteractionType.HORSE_SADDLE_2,
        "horse_hairstyle" => InteractionType.HORSE_HAIRSTYLE,
        "horse_body_dye" => InteractionType.HORSE_BODY_DYE,
        "horse_hair_dye" => InteractionType.HORSE_HAIR_DYE,
        "badge" => InteractionType.BADGE,
        "trampoline" => InteractionType.TRAMPOLINE,
        "treadmill" => InteractionType.TREADMILL,
        "crosstrainer" => InteractionType.CROSSTRAINER,
        _ => InteractionType.NONE,
    };
}
