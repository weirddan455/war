#include <gdnative_api_struct.gen.h>
#include <stdlib.h>

#define NUM_CELLS_X 16
#define NUM_CELLS_Y 9

typedef struct Node {
    int x;
    int y;
    int cost;
    struct Node *next;
} Node;

typedef struct Neighbor {
    int x;
    int y;
} Neighbor;

typedef struct CameFromNode {
    int x;
    int y;
    int from_x;
    int from_y;
    int cost;
    struct CameFromNode *next;
} CameFromNode;

typedef struct PathNode {
    int x;
    int y;
    struct PathNode *next;
} PathNode;

const godot_gdnative_core_api_struct *api = NULL;
const godot_gdnative_ext_nativescript_api_struct *nativescript_api = NULL;

void *pathfinding_constructor(godot_object *instance, void *method_data);
void pathfinding_destructor(godot_object *instance, void *method_data, void *user_data);
godot_variant get_movable_cells(godot_object *instance, void *method_data, void *user_data, int num_args, godot_variant **args);
godot_variant get_new_path(godot_object *instance, void *method_data, void *user_data, int num_args, godot_variant **args);
int get_cell_cost(godot_variant *tile_map, int x, int y);
void get_neighbors(Neighbor neighbors[], int x, int y);
Node *create_node(int x, int y, int cost);
CameFromNode *create_came_from_node(int x, int y, int from_x, int from_y, int cost);
int find_node(Node *node, int x, int y);
int find_came_from_node(CameFromNode *node, int x, int y);
void priority_push(Node **head, int x, int y, int cost);
void priority_pop(Node **head);

void GDN_EXPORT godot_gdnative_init(godot_gdnative_init_options *options) {
    api = options->api_struct;
    for (int i = 0; i < api->num_extensions; i++) {
        if (api->extensions[i]->type == GDNATIVE_EXT_NATIVESCRIPT) {
            nativescript_api = (godot_gdnative_ext_nativescript_api_struct *)api->extensions[i];
        }
    }
}

void GDN_EXPORT godot_gdnative_terminate(godot_gdnative_terminate_options *options) {
    api = NULL;
    nativescript_api = NULL;
}

void GDN_EXPORT godot_nativescript_init(void *handle) {
    godot_instance_create_func create = {NULL, NULL, NULL};
    create.create_func = &pathfinding_constructor;

    godot_instance_destroy_func destroy = {NULL, NULL, NULL};
    destroy.destroy_func = &pathfinding_destructor;

    nativescript_api->godot_nativescript_register_class(handle, "Pathfinding", "Reference", create, destroy);

    godot_instance_method method = {NULL, NULL, NULL};
    method.method = &get_movable_cells;
    godot_method_attributes attributes = {GODOT_METHOD_RPC_MODE_DISABLED};
    nativescript_api->godot_nativescript_register_method(handle, "Pathfinding", "get_movable_cells", attributes, method);

    method.method = &get_new_path;
    nativescript_api->godot_nativescript_register_method(handle, "Pathfinding", "get_new_path", attributes, method);
}

void *pathfinding_constructor(godot_object *instance, void *method_data) {
    return NULL;
}

void pathfinding_destructor(godot_object *instance, void *method_data, void *user_data) {
}

godot_variant get_movable_cells(godot_object *instance, void *method_data, void *user_data, int num_args, godot_variant **args) {
    int x = api->godot_variant_as_int(args[0]);
    int y = api->godot_variant_as_int(args[1]);
    int range = api->godot_variant_as_int(args[2]);
    godot_variant *tile_map = args[3];

    Node *frontier = create_node(x, y, 0);
    Node *cost_so_far_head = create_node(x, y, 0);
    Node *cost_so_far_tail = cost_so_far_head;
    Neighbor neighbors[5];

    while (frontier != NULL) {
        x = frontier->x;
        y = frontier->y;
        int cur_cost = find_node(cost_so_far_head, x, y);
        priority_pop(&frontier);
        get_neighbors(neighbors, x , y);
        for (int i = 0; neighbors[i].x < 9000; i++) {
            int new_cost = cur_cost + get_cell_cost(tile_map, neighbors[i].x, neighbors[i].y);
            if (new_cost <= range && find_node(cost_so_far_head, neighbors[i].x, neighbors[i].y) == -1) {
                Node *new_node = create_node(neighbors[i].x, neighbors[i].y, new_cost);
                cost_so_far_tail->next = new_node;
                cost_so_far_tail = new_node;
                priority_push(&frontier, neighbors[i].x, neighbors[i].y, new_cost);
            }
        }
    }

    godot_vector2 vec;
    godot_variant key, value;
    godot_dictionary dict;
    api->godot_dictionary_new(&dict);
    api->godot_variant_new_nil(&value);
    while (cost_so_far_head != NULL) {
        api->godot_vector2_new(&vec, cost_so_far_head->x, cost_so_far_head->y);
        api->godot_variant_new_vector2(&key, &vec);
        api->godot_dictionary_set(&dict, &key, &value);
        api->godot_variant_destroy(&key);
        Node *next = cost_so_far_head->next;
        api->godot_free(cost_so_far_head);
        cost_so_far_head = next;
    }
    api->godot_variant_destroy(&value);

    godot_variant ret;
    api->godot_variant_new_dictionary(&ret, &dict);
    api->godot_dictionary_destroy(&dict);
    return ret;
}

godot_variant get_new_path(godot_object *instance, void *method_data, void *user_data, int num_args, godot_variant **args) {
    int start_x = api->godot_variant_as_int(args[0]);
    int start_y = api->godot_variant_as_int(args[1]);
    int goal_x = api->godot_variant_as_int(args[2]);
    int goal_y = api->godot_variant_as_int(args[3]);
    godot_variant *tile_map = args[4];

    Node *frontier = create_node(start_x, start_y, 0);
    CameFromNode *list_head = create_came_from_node(start_x, start_y, 9001, 9001, 0);
    CameFromNode *list_tail = list_head;
    Neighbor neighbors[5];

    while (frontier != NULL) {
        int x = frontier->x;
        int y = frontier->y;
        if (x == goal_x && y == goal_y) {
            while (frontier != NULL) {
                priority_pop(&frontier);
            }
            break;
        }
        int cur_cost = find_came_from_node(list_head, x, y);
        priority_pop(&frontier);
        get_neighbors(neighbors, x , y);
        for (int i = 0; neighbors[i].x < 9000; i++) {
            int new_cost = cur_cost + get_cell_cost(tile_map, neighbors[i].x, neighbors[i].y);
            if (find_came_from_node(list_head, neighbors[i].x, neighbors[i].y) == -1) {
                CameFromNode *new_node = create_came_from_node(neighbors[i].x, neighbors[i].y, x, y, new_cost);
                list_tail->next = new_node;
                list_tail = new_node;
                priority_push(&frontier, neighbors[i].x, neighbors[i].y, new_cost + abs(goal_x - neighbors[i].x) + abs(goal_y - neighbors[i].y));
            }
        }
    }

    PathNode *path_head = api->godot_alloc(sizeof(PathNode));
    path_head->x = goal_x;
    path_head->y = goal_y;
    path_head->next = NULL;
    PathNode *path_tail = path_head;
    while (path_tail->x != start_x || path_tail->y != start_y) {
        CameFromNode *list_cur = list_head;
        while (list_cur->x != path_tail->x || list_cur->y != path_tail->y) {
            list_cur = list_cur->next;
        }
        PathNode *node = api->godot_alloc(sizeof(PathNode));
        node->x = list_cur->from_x;
        node->y = list_cur->from_y;
        node->next = NULL;
        path_tail->next = node;
        path_tail = node;
    }

    while (list_head != NULL) {
        CameFromNode *next = list_head->next;
        api->godot_free(list_head);
        list_head = next;
    }

    godot_vector2 vec;
    godot_variant var;
    godot_array arr;
    api->godot_array_new(&arr);
    while (path_head != NULL) {
        api->godot_vector2_new(&vec, path_head->x, path_head->y);
        api->godot_variant_new_vector2(&var, &vec);
        api->godot_array_push_front(&arr, &var);
        api->godot_variant_destroy(&var);
        PathNode *next = path_head->next;
        api->godot_free(path_head);
        path_head = next;
    }

    godot_variant ret;
    api->godot_variant_new_array(&ret, &arr);
    api->godot_array_destroy(&arr);
    return ret;
}

int get_cell_cost(godot_variant *tile_map, int x, int y) {
    godot_string method_name;
    godot_variant arg0, arg1;
    const godot_variant *args[2];
    godot_variant_call_error error;

    api->godot_string_new(&method_name);
    api->godot_string_parse_utf8(&method_name, "get_cell");
    api->godot_variant_new_int(&arg0, x);
    api->godot_variant_new_int(&arg1, y);
    args[0] = &arg0;
    args[1] = &arg1;

    godot_variant var_tile = api->godot_variant_call(tile_map, &method_name, args, 2, &error);
    int tile = api->godot_variant_as_int(&var_tile);

    api->godot_string_destroy(&method_name);
    api->godot_variant_destroy(&arg0);
    api->godot_variant_destroy(&arg1);
    api->godot_variant_destroy(&var_tile);

    int cost;
    switch(tile) {
        case 0:
            cost = 1;
            break;
        case 1:
            cost = 2;
            break;
        default:
            cost = 9001;
            break;
    }
    return cost;
}

void get_neighbors(Neighbor neighbors[], int x, int y) {
    int num_neighbors = 0;
    if (x + 1 < NUM_CELLS_X) {
        neighbors[num_neighbors].x = x + 1;
        neighbors[num_neighbors].y = y;
        num_neighbors++;
    }
    if (x - 1 >= 0) {
        neighbors[num_neighbors].x = x - 1;
        neighbors[num_neighbors].y = y;
        num_neighbors++;
    }
    if (y + 1 < NUM_CELLS_Y) {
        neighbors[num_neighbors].x = x;
        neighbors[num_neighbors].y = y + 1;
        num_neighbors++;
    }
    if (y - 1 >= 0) {
        neighbors[num_neighbors].x = x;
        neighbors[num_neighbors].y = y - 1;
        num_neighbors++;
    }
    neighbors[num_neighbors].x = 9001;
}

Node *create_node(int x, int y, int cost) {
    Node *node = api->godot_alloc(sizeof(Node));
    node->x = x;
    node->y = y;
    node->cost = cost;
    node->next = NULL;
    return node;
}

CameFromNode *create_came_from_node(int x, int y, int from_x, int from_y, int cost) {
    CameFromNode *node = api->godot_alloc(sizeof(CameFromNode));
    node->x = x;
    node->y = y;
    node->from_x = from_x;
    node->from_y = from_y;
    node->cost = cost;
    node->next = NULL;
    return node;
}

int find_node(Node *node, int x, int y) {
    while (node != NULL && (node->x != x || node->y != y)) {
        node = node->next;
    }
    if (node == NULL) {
        return -1;
    }
    return node->cost;
}

int find_came_from_node(CameFromNode *node, int x, int y) {
    while (node != NULL && (node->x != x || node->y != y)) {
        node = node->next;
    }
    if (node == NULL) {
        return -1;
    }
    return node->cost;
}

void priority_push(Node **head, int x, int y, int cost) {
    Node *node = create_node(x, y, cost);
    if (*head == NULL || (*head)->cost > cost) {
        node->next = *head;
        *head = node;
    } else {
        Node *cur = *head;
        while (cur->next != NULL && cur->next->cost <= cost) {
            cur = cur->next;
        }
        node->next = cur->next;
        cur->next = node;
    }
}

void priority_pop(Node **head) {
    Node *node = *head;
    *head = (*head)->next;
    api->godot_free(node);
}
