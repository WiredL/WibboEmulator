namespace WibboEmulator.Games.Items;

public enum InteractionType
{
    // Wired Trigger
    TRIGGER_AVATAR_ENTERS_ROOM,
    TRIGGER_AVATAR_SAYS_SOMETHING,
    TRIGGER_BOT_REACHED_AVTR,
    TRIGGER_BOT_REACHED_STF,
    TRIGGER_COLLISION,
    TRIGGER_COLLISION_USER,
    TRIGGER_COLLISION_USER_SELF,
    TRIGGER_COMMAND,
    TRIGGER_COMMAND_SELF,
    TRIGGER_GAME_ENDS,
    TRIGGER_GAME_STARTS,
    TRIGGER_ONCE,
    TRIGGER_PERIODICALLY,
    TRIGGER_PERIODICALLY_LONG,
    TRIGGER_SCORE_ACHIEVED,
    TRIGGER_STATE_CHANGED,
    TRIGGER_WALK_OFF_FURNI,
    TRIGGER_WALK_ON_FURNI,
    TRIGGER_AVATAR_EXIT,
    TRIGGER_USER_CLICK,
    TRIGGER_USER_CLICK_SELF,

    // Wired Action
    ACTION_BOT_CLOTHES,
    ACTION_BOT_FOLLOW_AVATAR,
    ACTION_BOT_GIVE_HANDITEM,
    ACTION_BOT_MOVE,
    ACTION_BOT_TALK,
    ACTION_BOT_TALK_TO_AVATAR,
    ACTION_BOT_TELEPORT,
    ACTION_CALL_STACKS,
    ACTION_CHASE,
    ACTION_COLLISION_CASE,
    ACTION_COLLISION_ITEM,
    ACTION_COLLISION_TEAM,
    ACTION_ENDGAME_TEAM,
    ACTION_FLEE,
    ACTION_GIVE_REWARD,
    ACTION_GIVE_SCORE,
    ACTION_GIVE_SCORE_TM,
    ACTION_JOIN_TEAM,
    ACTION_KICK_USER,
    ACTION_LEAVE_TEAM,
    ACTION_MOVE_ROTATE,
    ACTION_MOVE_TO_DIR,
    ACTION_POS_RESET,
    ACTION_RESET_TIMER,
    ACTION_SUPER_WIRED,
    ACTION_TELEPORT_TO,
    ACTION_TOGGLE_STATE,
    ACTION_USER_MOVE,
    ACTION_SHOW_MESSAGE,
    ACTION_TRIDIMENSION,
    ACTION_TELEPORT_FURNI,
    ACTION_ROOM_MESSAGE,
    ACTION_GIVE_POINTS_HIGHSCORE,
    ACTION_RESET_POINTS_HIGHSCORE,

    // Wired Condition
    CONDITION_ACTOR_IN_GROUP,
    CONDITION_ACTOR_IN_TEAM,
    CONDITION_DATE_RNG_ACTIVE,
    CONDITION_FURNIS_HAVE_NO_USERS,
    CONDITION_FURNIS_HAVE_USERS,
    CONDITION_HAS_FURNI_ON_FURNI,
    CONDITION_HAS_FURNI_ON_FURNI_NEGATIVE,
    CONDITION_HAS_HANDITEM,
    CONDITION_NOT_IN_GROUP,
    CONDITION_NOT_IN_TEAM,
    CONDITION_NOT_STUFF_IS,
    CONDITION_NOT_COLLISION_IS,
    CONDITION_COLLISION_IS,
    CONDITION_NOT_USER_COUNT,
    CONDITION_STATE_POS,
    CONDITION_STATE_POS_NEGATIVE,
    CONDITION_STUFF_IS,
    CONDITION_SUPER_WIRED,
    CONDITION_TIME_LESS_THAN,
    CONDITION_TIME_MORE_THAN,
    CONDITION_TRIGGER_ON_FURNI,
    CONDITION_TRIGGER_ON_FURNI_NEGATIVE,
    CONDITION_USER_COUNT_IN,
    CONDITION_COMPARE_HIGHSCORE,

    // Wired Special
    HIGH_SCORE,
    HIGH_SCORE_POINTS,
    SPECIAL_RANDOM,
    SPECIAL_UNSEEN,
    SPECIAL_ANIMATE,
    SPECIAL_OR_EVAL,
    WIRED_ITEM,

    // Banzai
    BANZAI_BLOB_2,
    BANZAI_BLOB,
    BANZAI_FLOOR,
    BANZAI_GATE_BLUE,
    BANZAI_GATE_GREEN,
    BANZAI_GATE_RED,
    BANZAI_GATE_YELLOW,
    BANZAI_PUCK,
    BANZAI_PYRAMID,
    BANZAI_SCORE_BLUE,
    BANZAI_SCORE_GREEN,
    BANZAI_SCORE_RED,
    BANZAI_SCORE_YELLOW,
    BANZAI_TELE,

    // Foot
    FOOTBALL,
    FOOTBALL_GATE,
    FOOTBALL_COUNTER_BLUE,
    FOOTBALL_COUNTER_GREEN,
    FOOTBALL_COUNTER_RED,
    FOOTBALL_COUNTER_YELLOW,
    FOOTBALL_GOAL_BLUE,
    FOOTBALL_GOAL_GREEN,
    FOOTBALL_GOAL_RED,
    FOOTBALL_GOAL_YELLOW,

    // Freeze
    FREEZE_BLUE_COUNTER,
    FREEZE_BLUE_GATE,
    FREEZE_EXIT,
    FREEZE_GREEN_COUNTER,
    FREEZE_GREEN_GATE,
    FREEZE_RED_COUNTER,
    FREEZE_RED_GATE,
    FREEZE_TILE,
    FREEZE_TILE_BLOCK,
    FREEZE_YELLOW_COUNTER,
    FREEZE_YELLOW_GATE,

    CHRONO_TIMER,

    // Horse
    HORSE_BODY_DYE,
    HORSE_HAIR_DYE,
    HORSE_HAIRSTYLE,
    HORSE_SADDLE_1,
    HORSE_SADDLE_2,
    HORSE_JUMP,

    // LootBox
    LEGEND_BOX,
    LOOTBOX_2022,
    DELUXE_BOX,
    EXTRA_BOX,
    BADGE_BOX,

    TELEPORT,
    TELEPORT_ARROW,

    GUILD_GATE,
    GUILD_ITEM,

    HALLOWEENPOOL,
    ICESKATES,
    POOL,
    NORMSLASKATES,
    TREADMILL,
    TRAMPOLINE,
    LOWPOOL,
    CROSSTRAINER,

    PET,
    BOT,

    // Other
    NONE,
    GATE,
    GIFT,
    GIFT_BANNER,
    TROC_BANNER,
    LTD_BIRTHDAY_2024,

    HABBOWHEEL,

    PHOTO,
    POSTIT,
    MOODLIGHT,

    TONER,
    JUKEBOX,
    LOVELOCK,
    LOVESHUFFLER,
    MANNEQUIN,
    ONEWAYGATE,

    PILE_MAGIC,
    PRESSURE_PAD,
    PUZZLE_BOX,
    ROLLER,
    SCORE_BOARD,

    TROPHY,
    TV_YOUTUBE,
    VENDING_ENABLE_MACHINE,
    VENDING_MACHINE,
    ADS_BACKGROUND,
    FLOOR_SWITCH,
    ALERT,

    BADGE,
    BADGE_DISPLAY,
    BADGE_TROC,

    BED,
    BOTTLE,
    CRACKABLE,
    DICE,

    EXCHANGE,
    EXCHANGE_TREE,
    EXCHANGE_TREE_CLASSIC,
    EXCHANGE_TREE_EPIC,
    EXCHANGE_TREE_LEGEND,

    PREMIUM_CLASSIC,
    PREMIUM_EPIC,
    PREMIUM_LEGEND,

    FLOOR,
    LANDSCAPE,
    WALLPAPER,
}
